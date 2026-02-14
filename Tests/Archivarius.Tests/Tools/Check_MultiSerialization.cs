using System.Collections.Generic;
using System.Threading.Tasks;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_MultiSerialization
    {
        [Test]
        public async Task Test_Polymorphic()
        {
            MultiSerializer serializer = new MultiSerializer(() => new TypenameBasedTypeSerializer());
            MultiDeserializer deserializer = new MultiDeserializer(() => new TypenameBasedTypeDeserializer());
            await Check(serializer, deserializer);
        }
        
        [Test]
        public async Task Test_Monomorphic()
        {
            MultiSerializer serializer = new MultiSerializer();
            MultiDeserializer deserializer = new MultiDeserializer();
            await Check(serializer, deserializer);
        }

        private async Task Check(MultiSerializer serializer, MultiDeserializer deserializer)
        {
            List<Task> tasks = new();

            for (int i = 0; i < 10000; ++i)
            {
                int k = i;
                Task task = Task.Run(async () =>
                {
                    DataClass data = new DataClass() { Value = k };
                    var bytes = await serializer.SerializeClassAsync(data);
                    await Task.Yield();
                    var newData = await deserializer.DeserializeClass<DataClass>(bytes);
                    Assert.IsTrue(newData.Item1);
                    Assert.IsTrue(k == newData.Item2!.Value);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private class DataClass : IDataStruct
        {
            public int Value;

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Value);
            }
        }
    }
}