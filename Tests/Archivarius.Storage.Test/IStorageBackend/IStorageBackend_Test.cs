using System;
using System.Linq;
using System.Threading.Tasks;
using Actuarius.Memory;
using Archivarius.Storage.Remote;
using Archivarius.Storage.Test.StorageBackend;
using Pontifex.Api;
using Pontifex.Transports.Direct;
using Scriba;
using Scriba.Consumers;

namespace Archivarius.Storage.Test
{
    public class IStorageBackend_Test
    {
        [Test]
        public async Task Test_File()
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
        
        [Test]
        public async Task Test_Remote()
        {
            var commands = CommandsGenerator.Generate(100000, 123);
            Tester<IStorageBackend> tester = new();

            Log.AddConsumer(new ConsoleConsumer());
            ILogger logger = new StaticLogger();

            IStorageBackend remoteBackend;
            {
                AckRawDirectServer directServer = new AckRawDirectServer("test", logger, MemoryRental.Shared);
                RemoteStorageBackendServer backend = new RemoteStorageBackendServer(new InMemoryStorageBackend());
                backend.Setup(directServer);
                
                AckRawDirectClient directClient = new AckRawDirectClient("test", logger, MemoryRental.Shared);
                remoteBackend = RemoteStorageBackendClient.Construct(directClient) ?? throw new Exception();
            }
            
            IStorageBackend etalon = new InMemoryStorageBackend();
            
            remoteBackend.ThrowExceptions = false;
            etalon.ThrowExceptions = false;
            var res = await tester.Run(commands, remoteBackend, etalon);
            Assert.That(res, Is.True);
        }
    }
}