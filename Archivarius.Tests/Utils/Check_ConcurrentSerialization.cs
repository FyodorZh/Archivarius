using System.Collections.Generic;
using System.Threading.Tasks;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_ConcurrentSerialization
    {
        [Test]
        public async Task Test()
        {
            ConcurrentSerialization center = new ConcurrentSerialization(
                () => new TypenameBasedTypeSerializer(),
                () => new TypenameBasedTypeDeserializer());

            List<Task> tasks = new();

            for (int i = 0; i < 10000; ++i)
            {
                int k = i;
                Task task = Task.Run(async () =>
                {
                    DataClass data = new DataClass() { Value = k };
                    var bytes = await center.SerializeClass(data);
                    await Task.Yield();
                    var newData = await center.DeserializeClass<DataClass>(bytes);
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