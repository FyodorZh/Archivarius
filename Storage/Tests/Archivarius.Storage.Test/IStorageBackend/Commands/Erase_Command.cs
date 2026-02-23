using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class Erase_Command : StorageBackendTestCommand<bool>
    {
        private readonly FilePath _path;

        public Erase_Command(FilePath path)
        {
            _path = path;
        }
        
        protected override Task<bool> InvokeOnSubject(IStorageBackend subject)
        {
            return subject.Erase(_path);
        }
    }
}