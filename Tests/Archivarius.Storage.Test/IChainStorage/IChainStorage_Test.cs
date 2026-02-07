using System.Threading.Tasks;
using Archivarius.TypeSerializers;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class IChainStorage_Test
    {
        [Test]
        public async Task Test_BigChainStorage()
        {
            CommandsGenerator commandsGenerator = new();
            var commands = commandsGenerator.Generate(10000, 1234);

            var etalon = new InMemoryChainStorage<CommandsGenerator.Payload>();
            var testSubject = BigChainStorage<CommandsGenerator.Payload>.ConstructNew(
                new KeyValueStorage(
                    new InMemoryStorageBackend(),
                    new TypenameBasedTypeSerializer(),
                    new TypenameBasedTypeDeserializer()),
                DirPath.Root,
                5,
                5);

            Tester<IChainStorage<CommandsGenerator.Payload>> tester = new();
            bool res = await tester.Run(commands, testSubject, etalon);
            Assert.That(res, Is.True);
        }
        
        [Test]
        public async Task Test_ChainStorage()
        {
            CommandsGenerator commandsGenerator = new();
            var commands = commandsGenerator.Generate(10000, 1234);

            var etalon = new InMemoryChainStorage<CommandsGenerator.Payload>();
            var testSubject = ChainStorage<CommandsGenerator.Payload>.ConstructNew(
                new KeyValueStorage(
                    new InMemoryStorageBackend(),
                    new TypenameBasedTypeSerializer(),
                    new TypenameBasedTypeDeserializer()),
                DirPath.Root,
                10);

            Tester<IChainStorage<CommandsGenerator.Payload>> tester = new();
            bool res = await tester.Run(commands, testSubject, etalon);
            Assert.That(res, Is.True);
        }
    }
}