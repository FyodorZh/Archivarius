using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Actuarius.Collections;
using Actuarius.Concurrent;
using Actuarius.Memory;

namespace Archivarius.Storage.Remote
{
    internal enum CommandType
    {
        Read, IsExists, GetSubPath, Write, Erase
    }
    
    public class StorageBackendLogic<TUserData> : IDisposable
    {
        private readonly IReadOnlySyncStorageBackend _roStorageBackend;
        private readonly ISyncStorageBackend? _storageBackend;
        
        private readonly IMemoryRental _memoryRental;
        private readonly Action<double>? _updateWorkloadMetric;

        private readonly MemoryStream _readBuffer = new();
        private IMultiRefByteArray? _readData;
        
        private readonly ManualResetEventSlim _resetEvent;
        private readonly ConcurrentQueueValve<Command> _commandsQueue = new (new SystemConcurrentQueue<Command>(), e => e.Free());
        
        private readonly SystemConcurrentStack<TUserData> _freeUserData = new SystemConcurrentStack<TUserData>();
        
        private volatile bool _stop;

        public StorageBackendLogic(ISyncStorageBackend storageBackend, IMemoryRental memoryRental, Action<double>? updateWorkloadMetric)
            :this((IReadOnlySyncStorageBackend)storageBackend, memoryRental, updateWorkloadMetric)
        {
            _storageBackend = storageBackend;
        }
        
        public StorageBackendLogic(IReadOnlySyncStorageBackend storageBackend, IMemoryRental memoryRental, Action<double>? updateWorkloadMetric)
        {
            storageBackend.ThrowExceptions = true;
            
            _roStorageBackend = storageBackend;
            _resetEvent = new ManualResetEventSlim(false);

            _memoryRental = memoryRental;
            _updateWorkloadMetric = updateWorkloadMetric;

            Thread thread = new Thread(Work, 128 * 1024);
            thread.Start();
        }
        
        public TUserData? GetFreeUserData()
        {
            if (_freeUserData.TryPop(out var userData))
            {
                return userData;
            }
            return default;
        }
        
        public void Dispose()
        {
            _commandsQueue.CloseValve();
            _stop = true;
            _resetEvent.Set();
        }

        private void Work()
        {
            Stopwatch totalTime = new Stopwatch();
            Stopwatch workTime = new Stopwatch();
            while (true)
            {
                totalTime.Restart();
                _resetEvent.Wait();
                _resetEvent.Reset();
                workTime.Restart();
                if (_stop)
                {
                    break;
                }

                while (_commandsQueue.TryPop(out Command command))
                {
                    if (_stop)
                    {
                        break;
                    }
                    command.Run(this);
                }

                if (_updateWorkloadMetric != null)
                {
                    double total = totalTime.Elapsed.TotalSeconds;
                    if (total > 1e-5)
                    {
                        double work = workTime.Elapsed.TotalSeconds;
                        double k = work / total;
                        _updateWorkloadMetric.Invoke(k);
                    }
                }
            }
        }
        
        private void EnqueueCommand(Command command)
        {
            _commandsQueue.Put(command);
            _resetEvent.Set();
        }

        public void Read(FilePath path, TUserData userData, Action<IMultiRefByteArray?, TUserData> onFinish, Action<Exception, TUserData> onFail)
        {
            var command = Command.Read(path, userData, onFinish, onFail);
            EnqueueCommand(command);
        }

        public void IsExists(FilePath path, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
        {
            var command = Command.IsExists(path, userData, onFinish, onFail);
            EnqueueCommand(command);
        }

        public void GetSubPath(DirPath path, TUserData userData, Action<IReadOnlyList<FilePath>, TUserData> onFinish, Action<Exception, TUserData> onFail)
        {
            var command = Command.GetSubPath(path, userData, onFinish, onFail);
            EnqueueCommand(command);
        }

        public void Write(FilePath path, IMultiRefByteArray bytesToWrite, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
        {
            var command = Command.Write(path, bytesToWrite, userData, onFinish, onFail);
            EnqueueCommand(command);
        }

        public void Erase(FilePath path, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
        {
            var command = Command.Erase(path, userData, onFinish, onFail);
            EnqueueCommand(command);
        }
        
        private struct Command
        {
            private CommandType _type;
            private TUserData _userData;
            private FilePath? _filePath;
            private DirPath? _dirPath;

            private IMultiRefByteArray? _bytesToWrite;

            private Action<bool, TUserData>? _boolFinish;
            private Action<IReadOnlyList<FilePath>, TUserData>? _pathsFinish;
            private Action<IMultiRefByteArray?, TUserData>? _bytesFinish;
            
            private Action<Exception, TUserData> _fail;

            public void Free()
            {
                _bytesToWrite?.Release();
                _bytesToWrite = null;
            }
            
            public static Command Read(FilePath path, TUserData userData, Action<IMultiRefByteArray?, TUserData> onFinish, Action<Exception, TUserData> onFail)
            {
                return new Command() { _type = CommandType.Read, _filePath = path, _userData = userData, _bytesFinish = onFinish, _fail = onFail};
            }
            
            public static Command IsExists(FilePath path, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
            {
                return new Command() { _type = CommandType.IsExists, _filePath = path, _userData = userData, _boolFinish = onFinish, _fail = onFail};
            }

            public static Command GetSubPath(DirPath path, TUserData userData, Action<IReadOnlyList<FilePath>, TUserData> onFinish, Action<Exception, TUserData> onFail)
            {
                return new Command() { _type = CommandType.GetSubPath, _dirPath = path, _userData = userData, _pathsFinish = onFinish, _fail = onFail};
            }
            
            public static Command Write(FilePath path, IMultiRefByteArray bytesToWrite, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
            {
                return new Command() { _type = CommandType.Write, _filePath = path, _userData = userData, _bytesToWrite = bytesToWrite, _boolFinish = onFinish, _fail = onFail};
            }
            
            public static Command Erase(FilePath path, TUserData userData, Action<bool, TUserData> onFinish, Action<Exception, TUserData> onFail)
            {
                return new Command() { _type = CommandType.Erase, _filePath = path, _userData = userData, _boolFinish = onFinish, _fail = onFail};
            }

            public void Run(StorageBackendLogic<TUserData> logic)
            {
                switch (_type)
                {
                    case CommandType.Read:
                    {
                        try
                        {
                            bool res = logic._roStorageBackend.Read(_filePath!, logic, (stream, l) =>
                            {
                                l._readBuffer.SetLength(0);
                                stream.CopyTo(l._readBuffer);
                                l._readData = l._memoryRental.ByteArraysPool.Acquire((int)l._readBuffer.Position);
                                l._readBuffer.Position = 0;
                                int count = l._readBuffer.Read(l._readData.Array, l._readData.Offset, l._readData.Count);
                                if (count != l._readData.Count)
                                {
                                    throw new Exception("Failed to read data. Internal error");
                                }
                            });
                            _bytesFinish!.Invoke(res ? logic._readData : null, _userData);
                        }
                        catch (Exception ex)
                        {
                            logic._readData?.Release();
                            _fail.Invoke(ex, _userData);
                        }
                        finally
                        {
                            logic._readData = null;
                        }
                        break;
                    }
                    case CommandType.IsExists:
                    {
                        try
                        {
                            bool res = logic._roStorageBackend.IsExists(_filePath!);
                            _boolFinish!.Invoke(res, _userData);
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                        }
                        break;
                    }
                    case CommandType.GetSubPath:
                    {
                        try
                        {
                            var res = logic._roStorageBackend.GetSubPaths(_dirPath!);
                            _pathsFinish!.Invoke(res, _userData);
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                        }
                        break;
                    }
                    case CommandType.Write:
                    {
                        try
                        {
                            if (logic._storageBackend == null)
                            {
                                _fail.Invoke(new InvalidOperationException("Failed to write to ReadOnly backend"), _userData);
                                return;
                            }

                            var res = logic._storageBackend.Write(_filePath!, _bytesToWrite!, (stream, bytes) => { stream.Write(bytes.Array, bytes.Offset, bytes.Count); });
                            _boolFinish!.Invoke(res, _userData);
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                        }
                        finally
                        {
                            _bytesToWrite!.Release();
                        }
                        break;
                    }
                    case CommandType.Erase:
                    {
                        try
                        {
                            if (logic._storageBackend == null)
                            {
                                _fail.Invoke(new InvalidOperationException("Failed to erase from ReadOnly backend"), _userData);
                                return;
                            }
                            var res = logic._storageBackend.Erase(_filePath!);
                            _boolFinish!.Invoke(res, _userData);
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                        }
                        break;
                    }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}