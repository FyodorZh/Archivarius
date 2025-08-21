using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public interface IStorageBackend
    {
        Task Write(FilePath path, Func<Stream, ValueTask> writer);
        Task Read(FilePath path, Func<Stream, Task> reader);
        Task Erase(FilePath path);
        Task<bool> IsExists(FilePath path);
        Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path);
    }

    public static class IStorageBackend_Ext
    {
        public static async Task<byte[]?> ReadAll(this IStorageBackend backend, FilePath path)
        {
            byte[]? bytes = null;
            
            await backend.Read(path, async stream =>
            {
                if (stream != null)
                {
                    var count = stream.Length - stream.Position;
                    bytes = new byte[count];
                    var actual = await stream.ReadAsync(bytes, 0, (int)count);
                    if (actual != count)
                    {
                        throw new Exception();
                    }
                }
            });

            return bytes;
        }
    }
}