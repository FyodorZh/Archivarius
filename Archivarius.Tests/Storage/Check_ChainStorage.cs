using System.Threading.Tasks;
using Archivarius.Storage;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_ChainStorage
    {
        public class Data : IDataStruct
        {
            public int Value;

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Value);
            }
        }
        
        [Test]
        public async Task Test()
        {
            IStorageBackend memory = new InMemoryStorageBackend();
            memory = new CompressedStorageBackend(memory);
            KeyValueStorage storage = new KeyValueStorage(memory,
                new TypenameBasedTypeSerializer(),
                new TypenameBasedTypeDeserializer());
            var chain = await storage.InitNewChain<Data>(DirPath.Root.Dir("chain"), 3);
            Assert.IsNotNull(chain);
            for (int i = 0; i < 10; ++i)
            {
                int id = await chain.Append(new Data() { Value = i });
                Assert.That(id, Is.EqualTo(i));
            }

            for (int i = 0; i < 10; ++i)
            {
                var data = await chain.GetAt(i);
                Assert.That(i, Is.EqualTo(data!.Value));
            }

            int pos = 0;
            await foreach (var list in chain.GetAll())
            {
                foreach (var el in list)
                {
                    Assert.That(pos, Is.EqualTo(el.Value));
                    pos += 1;
                }
            }
        }
    }
}