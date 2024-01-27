using System;
using Archivarius.BinaryBackend;
using Archivarius.JsonBackend;
using Archivarius.StructuredBinaryBackend;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class MainTest
    {
        [Test]
        public void StructuredBinaryTest()
        {
            DoTest(() => new StructuredBinaryWriter(), 
                w => new StructuredBinaryReader(w.ExtractData()));
        }
        
        [Test]
        public void BinaryTest()
        {
            DoTest(() => new BinaryWriter(), 
                w => new BinaryReader(w.GetBuffer()));
        }
        
        [Test]
        public void JsonTest()
        {
            DoTest(() => new JsonWriter(), 
                w => new JsonReader(w.ToJsonString()));
        }

        private void DoTest<TWriter>(Func<TWriter> getWriter, Func<TWriter, IReader> getReader)
            where TWriter : IWriter
        {
            var dataWriter = getWriter();
            var writer = new GraphSerializer(dataWriter, new TypenameBasedTypeSerializer(), null);

            ClassComposition? data = new ClassComposition(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

            writer.AddClass(ref data);

            var dataReader = getReader(dataWriter);
            var reader = new GraphDeserializer(dataReader, new TypenameBasedTypeDeserializer());
            reader.OnException += (ex) =>
            {
                Assert.Fail(ex.ToString());
            };

            ClassComposition? copy = null;
            reader.AddClass(ref copy);
            
            Assert.That(data!.Equals(copy), Is.True);
        }
    }
}