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
    public class StorageBackendLogic<TUserData> : IDisposable
    {
        private readonly IReadOnlySyncStorageBackend _roStorageBackend;
        private readonly ISyncStorageBackend? _storageBackend;
        
        private readonly IMemoryRental _memoryRental;

        private readonly MemoryStream _readBuffer = new();
        private IMultiRefByteArray? _readData;
        
        private readonly ManualResetEventSlim _resetEvent;
        private readonly ConcurrentQueueValve<Command> _commandsQueue = new (new SystemConcurrentQueue<Command>(), e => e.Free());
        
        private readonly SystemConcurrentStack<TUserData> _freeUserData = new SystemConcurrentStack<TUserData>();
        
        private volatile bool _stop;

        private readonly Action<CommandReport>? _commandReportProcessor;

        public StorageBackendLogic(ISyncStorageBackend storageBackend, IMemoryRental memoryRental, Action<CommandReport>? commandReportProcessor)
            :this((IReadOnlySyncStorageBackend)storageBackend, memoryRental, commandReportProcessor)
        {
            _storageBackend = storageBackend;
        }
        
        public StorageBackendLogic(IReadOnlySyncStorageBackend storageBackend, IMemoryRental memoryRental, Action<CommandReport>? commandReportProcessor)
        {
            storageBackend.ThrowExceptions = true;
            
            _roStorageBackend = storageBackend;
            _resetEvent = new ManualResetEventSlim(false);

            _memoryRental = memoryRental;

            _commandReportProcessor = commandReportProcessor;

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
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                _resetEvent.Wait();
                _resetEvent.Reset();
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
                    sw.Restart();
                    bool isOk = command.Run(this);
                    sw.Stop();
                    _commandReportProcessor?.Invoke(new CommandReport() { Type = command.Type, Success = isOk, Duration = sw.Elapsed });
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

            public CommandType Type => _type;

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

            public bool Run(StorageBackendLogic<TUserData> logic)
            {
                switch (_type)
                {
                    case CommandType.Read:
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
                            if (!res)
                            {
                                _fail.Invoke(new Exception("Failed to read data"), _userData);
                                return false;
                            }
                            _bytesFinish!.Invoke(logic._readData, _userData);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            logic._readData?.Release();
                            _fail.Invoke(ex, _userData);
                            return false;
                        }
                        finally
                        {
                            logic._readData = null;
                        }
                    case CommandType.IsExists:
                        try
                        {
                            bool res = logic._roStorageBackend.IsExists(_filePath!);
                            _boolFinish!.Invoke(res, _userData);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                            return false;
                        }
                    case CommandType.GetSubPath:
                        try
                        {
                            var res = logic._roStorageBackend.GetSubPaths(_dirPath!);
                            _pathsFinish!.Invoke(res, _userData);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                            return false;
                        }
                    case CommandType.Write:
                        try
                        {
                            if (logic._storageBackend == null)
                            {
                                _fail.Invoke(new InvalidOperationException("Failed to write to ReadOnly backend"), _userData);
                                return false;
                            }

                            var res = logic._storageBackend.Write(_filePath!, _bytesToWrite!, (stream, bytes) => { stream.Write(bytes.Array, bytes.Offset, bytes.Count); });
                            if (res)
                            {
                                _boolFinish!.Invoke(res, _userData);
                            }
                            else
                            {
                                _fail.Invoke(new Exception("Faield to write data"), _userData);
                            }

                            return res;
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                            return false;
                        }
                        finally
                        {
                            _bytesToWrite!.Release();
                        }
                    case CommandType.Erase:
                        try
                        {
                            if (logic._storageBackend == null)
                            {
                                _fail.Invoke(new InvalidOperationException("Failed to erase from ReadOnly backend"), _userData);
                                return false;
                            }
                            var res = logic._storageBackend.Erase(_filePath!);
                            _boolFinish!.Invoke(res, _userData);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _fail.Invoke(ex, _userData);
                            return false;
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}