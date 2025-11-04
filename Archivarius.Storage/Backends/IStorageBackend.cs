using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public interface IReadOnlyStorageBackend
    {
        /// <summary>
        /// Stream of all errors
        /// </summary>
        event Action<Exception> OnError;
        
        /// <summary>
        /// Should backend throw exceptions on errors or just return FALSE
        /// </summary>
        bool ThrowExceptions { get; set; }
        
        /// <summary>
        /// Read file content
        /// </summary>
        /// <param name="path"> File to read </param>
        /// <param name="reader"> Action that reads file content </param>
        /// <returns>
        /// TRUE - file content was read
        /// FALSE - file doesn't exist
        /// FALSE/Exception - something went wrong
        /// </returns>
        Task<bool> Read(FilePath path, Func<Stream, Task> reader);
        
        /// <summary>
        /// Check is file exists
        /// </summary>
        /// <param name="path"> File to check </param>
        /// <returns>
        /// TRUE - file exists
        /// FALSE - file doesn't exist
        /// FALSE/Exception - something went wrong
        /// </returns>
        Task<bool> IsExists(FilePath path);
        
        /// <summary>
        /// Returns list of files inside specified directory (and subdirectories)
        /// </summary>
        /// <param name="path"> Directory path </param>
        /// <returns>
        /// [files] - List of files
        /// []/Exception - if something went wrong
        /// </returns>
        Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path);
    }
    
    public interface IStorageBackend : IReadOnlyStorageBackend
    {
        /// <summary>
        /// Creates new file if it doesn't exist. Overwrite its content with new data 
        /// </summary>
        /// <param name="path"> File path </param>
        /// <param name="writer"> Action that owerwrites content</param>
        /// <returns>
        /// TRUE - file was successfully overwritten (and possibly created)
        /// FALSE/Exception - we don't know if file content was written
        /// </returns>
        Task<bool> Write(FilePath path, Func<Stream, Task> writer);
        
        /// <summary>
        /// Erase specified file record
        /// </summary>
        /// <param name="path"> Path to file record </param>
        /// <returns>
        /// TRUE - file was successfully deleted
        /// FALSE - file doesn't exist. 
        /// FALSE/Exception - we don't know if the file exists and can't be sure that it was deleted
        /// </returns>
        Task<bool> Erase(FilePath path);
    }

    public static class IStorageBackend_Ext
    {
        public static async Task<byte[]?> ReadAll(this IReadOnlyStorageBackend backend, FilePath path)
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