using System.Collections.Generic;
using Archivarius.StructuredBinaryBackend;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public struct SA : IDataStruct
    {
        public int x;

        public void Serialize(IOrderedSerializer serializer)
        {
            serializer.Add(ref x);
        }
    }

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
        public SA? pSA;

        public void Serialize(IOrderedSerializer serializer)
        {
            if (!serializer.IsWriter)
            {
                Assert.IsTrue(serializer.Version == this.Version);
            }
            serializer.Add(ref y);
            serializer.Add(ref hello);
            serializer.Add(ref data);
            serializer.Add(ref array);
            serializer.Add(ref shortList);
            serializer.Add(ref stringList);
            serializer.Add(ref pInt);
            serializer.Add(ref pSA);
            serializer.AddClass(ref self);
        }

        public byte Version => 134;
    }

    class Root : IDataStruct
    {
        public CA? a;
        public SA sa;

        public void Serialize(IOrderedSerializer serializer)
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
            var guid = typeof(Root).GUID;


            Root r0 = new Root();
            r0.sa = new SA();
            r0.sa.x = 3;
            r0.a = new CA();
            r0.a.self = r0.a;
            r0.a.y = 7;
            r0.a.hello = "hello";
            r0.a.data = new byte[] { 1, 2, 3 };
            r0.a.array = new short[] { 3, 400 };
            r0.a.shortList = new List<short>() { 1, 23, 5 };
            r0.a.stringList = new List<string?>() { "hello", null, "world" };

            StructuredBinaryReader dataReader;
            {
                var dataWriter = new StructuredBinaryWriter();
                var writer = new GraphSerializer(dataWriter, new TypenameBasedTypeSerializer(), null);

                writer.AddClass(ref r0!);

                dataReader = new StructuredBinaryReader(dataWriter.ExtractData());
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