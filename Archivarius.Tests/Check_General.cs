using System.Collections.Generic;
using Archivarius.BinaryBackend;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    class CA : IDataStruct, IVersionedData
    {
        public int y;
        public string? hello;
        public CA? self;
        public byte[]? data;
        public short[]? array;
        public List<short>? shortList;
        public List<string?>? stringList;
        public int? pInt;
        public StructInt? pSA;

        public void Serialize(ISerializer serializer)
        {
            if (!serializer.IsWriter)
            {
                Assert.IsTrue(serializer.Version == this.Version);
            }
            serializer.Add(ref y);
            serializer.Add(ref hello);
            serializer.Add(ref data);
            serializer.AddArray(ref array);
            serializer.AddList(ref shortList);
            serializer.AddList(ref stringList);
            serializer.Add(ref pInt);
            serializer.Add(ref pSA);
            serializer.AddClass(ref self);
        }

        public byte Version => 134;
    }

    class Root : IDataStruct
    {
        public CA? a;
        public StructInt sa;

        public void Serialize(ISerializer serializer)
        {
            serializer.AddClass(ref a);
            serializer.AddStruct(ref sa);
        }
    }

    [TestFixture]
    public class Check_General
    {
        [Test]
        public void DoTest()
        {
            Root r0 = new Root();
            r0.sa = new StructInt(3);
            r0.a = new CA();
            r0.a.self = r0.a;
            r0.a.y = 7;
            r0.a.hello = "hello";
            r0.a.data = new byte[] { 1, 2, 3 };
            r0.a.array = new short[] { 3, 400 };
            r0.a.shortList = new List<short>() { 1, 23, 5 };
            r0.a.stringList = new List<string?>() { "hello", null, "world" };

            BinaryReader dataReader;
            {
                var dataWriter = new BinaryWriter();
                var writer = new GraphSerializer(dataWriter, new TypenameBasedTypeSerializer(), null);

                writer.AddClass(ref r0!);

                dataReader = new BinaryReader(dataWriter.GetBuffer());
            }

            {
                var reader = new GraphDeserializer(dataReader, new TypenameBasedTypeDeserializer());

                reader.OnException += (ex) =>
                {
                    Assert.Fail(ex.ToString());
                };

                Root? r1 = null;
                reader.AddClass(ref r1);
            }
        }
    }
}