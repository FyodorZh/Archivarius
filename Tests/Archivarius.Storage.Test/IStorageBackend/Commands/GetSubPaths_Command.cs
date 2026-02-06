using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class GetSubPaths_Command : StorageBackendTestCommand<GetSubPaths_Command.PathArray>
    {
        private readonly DirPath _path;
        
        public GetSubPaths_Command(DirPath path)
        {
            _path = path;
        }
        
        protected override async Task<PathArray> InvokeOnSubject(IStorageBackend subject)
        {
            return new PathArray() { Paths = new List<FilePath>(await subject.GetSubPaths(_path)) };
        }
        
        public struct PathArray : IEquatable<PathArray>
        {
            public List<FilePath> Paths = [];
            public PathArray() { }

            public bool Equals(PathArray other)
            {
                return Paths.SequenceEqual(other.Paths);
            }
        }
    }
}