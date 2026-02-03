using System.Collections.Generic;
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
        public async Task Test_ChainStorage()
        {
            IStorageBackend memory = new InMemoryStorageBackend();
            memory = new CompressedStorageBackend(memory);
            KeyValueStorage storage = new KeyValueStorage(memory,
                new TypenameBasedTypeSerializer(),
                new TypenameBasedTypeDeserializer());
            var chain = await storage.CreateNewChain<Data>(DirPath.Root.Dir("chain"), 3);
            await Test1(chain);
        }

        [Test]
        public async Task Test_ChainStorage_GetMany()
        {
            IStorageBackend memory = new InMemoryStorageBackend();
            memory = new CompressedStorageBackend(memory);
            KeyValueStorage storage = new KeyValueStorage(memory,
                new TypenameBasedTypeSerializer(),
                new TypenameBasedTypeDeserializer());
            var chain = (await storage.CreateNewChain<Data>(DirPath.Root.Dir("chain"), 10));
            await Test2(chain);
        }
        
        [Test]
        public async Task Test_BigChainStorage()
        {
            IStorageBackend memory = new InMemoryStorageBackend();
            memory = new CompressedStorageBackend(memory);
            KeyValueStorage storage = new KeyValueStorage(memory,
                new TypenameBasedTypeSerializer(),
                new TypenameBasedTypeDeserializer());
            var chain = await BigChainStorage<Data>.LoadOrConstruct(storage, DirPath.Root.Dir("chain"), 3, 4);
            await Test1(chain);
        }

        [Test]
        public async Task Test_BigChainStorage_GetMany()
        {
            IStorageBackend memory = new InMemoryStorageBackend();
            memory = new CompressedStorageBackend(memory);
            KeyValueStorage storage = new KeyValueStorage(memory,
                new TypenameBasedTypeSerializer(),
                new TypenameBasedTypeDeserializer());
            var chain = await BigChainStorage<Data>.LoadOrConstruct(storage, DirPath.Root.Dir("chain"), 4, 3);
            await Test2(chain);
        }

        private async Task Test1(IChainStorage<Data>? chain)
        {
            int N = 100;
            Assert.IsNotNull(chain);
            for (int i = 0; i < N; ++i)
            {
                int id = await chain.Append(new Data() { Value = i });
                Assert.That(id, Is.EqualTo(i));
            }

            for (int i = 0; i < N; ++i)
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

        private async Task Test2(IChainStorage<Data>? chain)
        {
            Assert.IsNotNull(chain);
            
            int N = 100;
            for (int i = 0; i < N; ++i)
            {
                int id = await chain.Append(new Data() { Value = i });
                Assert.That(id, Is.EqualTo(i));
            }

            List<int> res = new();
            for (int a = 0; a < N; ++a)
            {
                for (int b = a; b < N; ++b)
                {
                    res.Clear();
                    await foreach (var list in chain.GetMany(a, b))
                    {
                        foreach (var el in list)
                        {
                            res.Add(el.Value);
                        }
                    }

                    int k = 0;
                    for (int i = a; i <= b; ++i)
                    {
                        Assert.That(res[k++], Is.EqualTo(i));
                    }
                }
            }
        }
    }
}