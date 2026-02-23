using System;
using System.Threading.Tasks;
using Archivarius.TypeSerializers;

namespace Archivarius.Storage.Test.ChainStorage
{
    [TestFixture]
    public class IChainStorage_Test
    {
        [Test]
        public async Task Test_BigChainStorage()
        {
            CommandsGenerator commandsGenerator = new();
            var commands = commandsGenerator.Generate(30000, 1234);

            var etalon = new InMemoryChainStorage<CommandsGenerator.Payload>();
            var etalonWrapper = new ChainStorageWrapper<CommandsGenerator.Payload>(() => Task.FromResult<IChainStorage<CommandsGenerator.Payload>>(etalon));
            await etalonWrapper.Reinit();
            
            var storage = new InMemoryStorageBackend();
            var testSubject = new ChainStorageWrapper<CommandsGenerator.Payload>(async () =>
                await BigChainStorage<CommandsGenerator.Payload>.LoadOrConstruct(
                    new KeyValueStorage(
                        storage,
                        new TypenameBasedTypeSerializer(),
                        new TypenameBasedTypeDeserializer()),
                    DirPath.Root, 10, 10));
            await testSubject.Reinit();

            Tester<ChainStorageWrapper<CommandsGenerator.Payload>> tester = new();
            bool res = await tester.Run(commands, testSubject, etalonWrapper);
            Assert.That(res, Is.True);
        }
        
        [Test]
        public async Task Test_BigChainStorage2()
        {
            CommandsGenerator commandsGenerator = new();
            var commands = commandsGenerator.Generate2(123);

            var etalon = new InMemoryChainStorage<CommandsGenerator.Payload>();
            var etalonWrapper = new ChainStorageWrapper<CommandsGenerator.Payload>(() => Task.FromResult<IChainStorage<CommandsGenerator.Payload>>(etalon));
            await etalonWrapper.Reinit();
            
            IStorageBackend storage = new InMemoryStorageBackend();
            storage = new StorageBackendThrottler(storage, TimeSpan.FromMilliseconds(10));
            var testSubject = new ChainStorageWrapper<CommandsGenerator.Payload>(async () =>
            {
                var res = await BigChainStorage<CommandsGenerator.Payload>.LoadOrConstruct(
                    new KeyValueStorage(
                        storage,
                        new TypenameBasedTypeSerializer(),
                        new TypenameBasedTypeDeserializer()),
                    DirPath.Root, 10, 10);
                res.MaintainCleanStorage = false;
                return res;
            });
            await testSubject.Reinit();

            Tester<ChainStorageWrapper<CommandsGenerator.Payload>> tester = new();
            bool res = await tester.Run(commands, testSubject, etalonWrapper);
            Assert.That(res, Is.True);
        }
        
        [Test]
        public async Task Test_ChainStorage()
        {
            CommandsGenerator commandsGenerator = new();
            var commands = commandsGenerator.Generate(10000, 1234);

            var etalon = new InMemoryChainStorage<CommandsGenerator.Payload>();
            var etalonWrapper = new ChainStorageWrapper<CommandsGenerator.Payload>(() => Task.FromResult<IChainStorage<CommandsGenerator.Payload>>(etalon));
            await etalonWrapper.Reinit();
            
            IStorageBackend storage = new InMemoryStorageBackend();
            //storage = new StorageBackendThrottler(storage, TimeSpan.FromMilliseconds(1));
            var testSubject = new ChainStorageWrapper<CommandsGenerator.Payload>(async () =>
                await ChainStorage<CommandsGenerator.Payload>.LoadOrConstruct(
                    new KeyValueStorage(
                        storage,
                        new TypenameBasedTypeSerializer(),
                        new TypenameBasedTypeDeserializer()),
                    DirPath.Root,
                    10));
            await testSubject.Reinit();
            
            Tester<ChainStorageWrapper<CommandsGenerator.Payload>> tester = new();
            bool res = await tester.Run(commands, testSubject, etalonWrapper);
            Assert.That(res, Is.True);
        }
    }
}