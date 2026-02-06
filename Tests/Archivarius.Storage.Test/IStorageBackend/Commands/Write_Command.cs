using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class Write_Command : StorageBackendTestCommand<bool>
    {
        private readonly FilePath _path;
        private readonly byte[] _data;

        public Write_Command(FilePath path, byte[] data)
        {
            _path = path;
            _data = data;
        }

        protected override Task<bool> InvokeOnSubject(IStorageBackend subject)
        {
            return subject.Write(_path, 0, (s, _) => s.WriteAsync(_data, 0, _data.Length));
        }
    }
}