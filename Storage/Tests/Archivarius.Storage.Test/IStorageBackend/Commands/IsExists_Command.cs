using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class IsExists_Command : StorageBackendTestCommand<bool>
    {
        private readonly FilePath _path;

        public IsExists_Command(FilePath path)
        {
            _path = path;
        }
        
        protected override Task<bool> InvokeOnSubject(IStorageBackend subject)
        {
            return subject.IsExists(_path);
        }
    }
}