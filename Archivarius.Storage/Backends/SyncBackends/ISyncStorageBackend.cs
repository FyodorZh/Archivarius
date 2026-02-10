using System;
using System.Collections.Generic;
using System.IO;

namespace Archivarius.Storage
{
    public interface IReadOnlySyncStorageBackend
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
        /// <param name="param"> Arbitrary user data</param>
        /// <param name="reader"> Action that reads file content </param>
        /// <returns>
        /// TRUE - file content was read
        /// FALSE - file doesn't exist
        /// FALSE/Exception - something went wrong
        /// </returns>
        bool Read<TParam>(FilePath path, TParam param, Action<Stream, TParam> reader);
        
        /// <summary>
        /// Check is file exists
        /// </summary>
        /// <param name="path"> File to check </param>
        /// <returns>
        /// TRUE - file exists
        /// FALSE - file doesn't exist
        /// FALSE/Exception - something went wrong
        /// </returns>
        bool IsExists(FilePath path);
        
        /// <summary>
        /// Returns a list of files inside the specified directory (and subdirectories)
        /// </summary>
        /// <param name="path"> Directory path </param>
        /// <returns>
        /// [files] - List of files
        /// []/Exception - if something went wrong
        /// </returns>
        IReadOnlyList<FilePath> GetSubPaths(DirPath path);
    }
    
    public interface ISyncStorageBackend : IReadOnlySyncStorageBackend
    {
        /// <summary>
        /// Creates a new file if it doesn't exist. Overwrite its content with new data 
        /// </summary>
        /// <param name="path"> File path </param>
        /// <param name="param"> Arbitrary user data</param>
        /// <param name="writer"> Action that owerwrites content</param>
        /// <returns>
        /// TRUE - file was successfully overwritten (and possibly created)
        /// FALSE/Exception - we don't know if file content was written
        /// </returns>
        bool Write<TParam>(FilePath path, TParam param, Action<Stream, TParam> writer);
        
        /// <summary>
        /// Erase specified file record
        /// </summary>
        /// <param name="path"> Path to file record </param>
        /// <returns>
        /// TRUE - file was successfully deleted
        /// FALSE - file doesn't exist. 
        /// FALSE/Exception - we don't know if the file exists and can't be sure that it was deleted
        /// </returns>
        bool Erase(FilePath path);
    }
}