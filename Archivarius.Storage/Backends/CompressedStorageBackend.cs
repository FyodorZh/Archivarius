using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Archivarius.Internals;
using Ionic.Zlib;

namespace Archivarius.Storage
{
    public class CompressedStorageBackend : IStorageBackend
    {
        private readonly IStorageBackend _storage;

        private readonly ObjectPool<Compressor> _compressors;
        private readonly ObjectPool<Decompressor> _decompressors;
        
        public event Action<Exception>? OnError;

        public CompressedStorageBackend(IStorageBackend storage)
        {
            _storage = storage;

            _compressors = new ObjectPool<Compressor>(() => new Compressor(), _ => { });
            _decompressors = new ObjectPool<Decompressor>(() => new Decompressor(), _ => { });
            storage.OnError += e => OnError?.Invoke(e);
        }

        async Task<bool> IStorageBackend.Write(FilePath path, Func<Stream, ValueTask> writer)
        {
            var compressor = await _compressors.GetAsync();
            try
            {
                await writer(compressor.PrepareToCompress());
                var compressedStream = compressor.Compress();
                return await _storage.Write(path, dst =>
                {
                    compressedStream.WriteTo(dst);
                    return default;
                });
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
            finally
            {
                await _compressors.ReleaseAsync(compressor);
            }
        }

        async Task<bool> IReadOnlyStorageBackend.Read(FilePath path, Func<Stream, Task> reader)
        {
            var decompressor = await _decompressors.GetAsync();
            try
            {
                MemoryStream? decompressed = null;
                if (!await _storage.Read(path, stream =>
                    {
                        decompressed = decompressor.Decompress(stream);
                        return Task.CompletedTask;
                    }))
                {
                    return false;
                }

                await reader.Invoke(decompressed ?? throw new NullReferenceException());
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;           
            }
            finally
            {
                await _decompressors.ReleaseAsync(decompressor);
            }
        }

        Task<bool> IStorageBackend.Erase(FilePath path)
        {
            return _storage.Erase(path);
        }

        public Task<bool> IsExists(FilePath path)
        {
            return _storage.IsExists(path);
        }

        public Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            return _storage.GetSubPaths(path);
        }

        private class Compressor
        {
            private readonly MemoryStream _srcStream;
            private readonly MemoryStream _compressedStream;
            private readonly DeflateStream _compressor;

            public Compressor()
            {
                _srcStream = new();
                _compressedStream = new();
                _compressor = new DeflateStream(_compressedStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                _compressor.FlushMode = FlushType.Full;
            }

            public Stream PrepareToCompress()
            {
                _srcStream.SetLength(0);
                return _srcStream;
            }

            public MemoryStream Compress()
            {
                _compressedStream.SetLength(0);
                _compressedStream.WriteByte(1); // compressed
                _srcStream.WriteTo(_compressor);
                _compressor.Flush();
                _compressedStream.Position = 0;

                if (_compressedStream.Length > _srcStream.Length)
                {
                    _compressedStream.WriteByte(0);
                    _srcStream.Position = 0;
                    _srcStream.WriteTo(_compressedStream);
                    _compressedStream.Position = 0;
                }

                return _compressedStream;
            }
        }

        private class Decompressor
        {
            private readonly MemoryStream _decompressedStream;
            private readonly DeflateStream _decompressor;
            private readonly byte[] _buffer = new byte[1024 * 8];

            public Decompressor()
            {
                _decompressedStream = new();
                _decompressor = new DeflateStream(_decompressedStream, CompressionMode.Decompress);
                _decompressor.FlushMode = FlushType.Full;
            }

            public MemoryStream Decompress(Stream compressedData)
            {
                _decompressedStream.SetLength(0);

                Stream dst;

                int flag = compressedData.ReadByte();
                if (flag == 0)
                {
                    dst = _decompressedStream;
                }
                else if (flag == 1)
                {
                    dst = _decompressor;
                }
                else
                {
                    throw new InvalidOperationException();
                }

                while (compressedData.Position < compressedData.Length)
                {
                    var count = compressedData.Read(_buffer, 0, _buffer.Length);
                    dst.Write(_buffer, 0, count);
                }

                if (flag == 1)
                {
                    _decompressor.Flush();
                }

                _decompressedStream.Position = 0;
                return _decompressedStream;
            }
        }
    }
}