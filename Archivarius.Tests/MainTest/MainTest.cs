using System;
using System.IO;
using Archivarius.BinaryBackend;
using Archivarius.JsonBackend;
using Archivarius.UnionDataListBackend;
using Archivarius.TypeSerializers;
using NUnit.Framework;
using BinaryReader = Archivarius.BinaryBackend.BinaryReader;
using BinaryWriter = Archivarius.BinaryBackend.BinaryWriter;

namespace Archivarius.Tests
{
    public class MainTest
    {
        [Test]
        public void UnionDataListTest()
        {
            DoTest(() => new UnionDataListWriter(), 
                w => new UnionDataListReader(w.CopyData()), true);
        }
        
        [Test]
        public void BinaryTest()
        {
            DoTest(() => new BinaryWriter(), 
                w => new BinaryReader(w.GetBuffer()), true);
            DoTest(() => new BinaryWriter(), 
                w => new BinaryReader(w.GetBuffer()), false);
        }
        
        [Test]
        public void BinaryStreamTest()
        {
            DoTest(() => new BinaryStreamWriter(new MemoryStream()), 
                w =>
                {
                    var stream = w.BaseStream;
                    stream.Position = 0;
                    return new BinaryStreamReader(stream);
                }, false);
        }
        
        [Test]
        public void JsonTest()
        {
            DoTest(() => new JsonWriter(), 
                w => new JsonReader(w.ToJsonString()), 
                true);
        }

        private void DoTest<TWriter>(Func<TWriter> getWriter, Func<TWriter, IReader> getReader, bool useSections)
            where TWriter : IWriter
        {
            DoTest(getWriter, getReader, useSections, true);
            DoTest(getWriter, getReader, useSections, false);
        }

        private void DoTest<TWriter>(Func<TWriter> getWriter, Func<TWriter, IReader> getReader, bool useSections, bool usePolymorphism)
            where TWriter : IWriter
        {
            var dataWriter = getWriter();
            GraphSerializer writer = usePolymorphism ? new GraphSerializer(dataWriter, new TypenameBasedTypeSerializer(), null, useSections)
                    : new GraphSerializer(dataWriter, useSections);

            ClassComposition? data = new ClassComposition(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

            writer.AddClass(ref data);

            var dataReader = getReader(dataWriter);
            GraphDeserializer reader = usePolymorphism ? new GraphDeserializer(dataReader, new TypenameBasedTypeDeserializer())
                    : new GraphDeserializer(dataReader);
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