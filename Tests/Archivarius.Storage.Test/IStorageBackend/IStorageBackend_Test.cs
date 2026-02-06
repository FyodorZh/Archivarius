using System.Linq;
using System.Threading.Tasks;
using Archivarius.Storage.Test.StorageBackend;

namespace Archivarius.Storage.Test
{
    public class IStorageBackend_Test
    {
        [Test]
        public async Task Test_InMemory_File()
        {
            var commands = CommandsGenerator.Generate(10000, 123);
            Tester<IStorageBackend> tester = new();
            var tempDir = System.IO.Path.GetTempPath() + "IStorageBackend_Test_7234289";
            if (System.IO.Directory.Exists(tempDir))
            {
                System.IO.Directory.Delete(tempDir, true);
            }
            System.IO.Directory.CreateDirectory(tempDir);
            FsStorageBackend backend = new(tempDir);
            InMemoryStorageBackend etalon = new();
            backend.ThrowExceptions = false;
            etalon.ThrowExceptions = false;
            var res = await tester.Run(commands, backend, etalon);
            Assert.That(res, Is.True);
        }
    }
}