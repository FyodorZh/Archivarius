using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class Check_Containers
    {
        [Test]
        public void TestArray()
        {
            ReaderWriterStream stream = new ReaderWriterStream();

            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, new TypenameBasedTypeSerializer());
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(stream, new TypenameBasedTypeDeserializer());

            int[]? nullArray = null;
            int[]? zeroArray = Array.Empty<int>();
            int[]? smallArray = { 1, 2, 3 };
            string?[]? stringArray = { "hello", "world" };
            (int, int)[]? pairs = { (1, 2), (3, 4) };

            serializer.Add(ref nullArray);
            serializer.Add(ref zeroArray);
            serializer.Add(ref smallArray);
            serializer.Add(ref stringArray);
            serializer.Add(ref pairs, (ISerializer s, ref (int, int) value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });

            nullArray = zeroArray = smallArray = null;
            stringArray = null;
            pairs = null;

            deserializer.Add(ref nullArray);
            deserializer.Add(ref zeroArray);
            deserializer.Add(ref smallArray);
            deserializer.Add(ref stringArray);
            deserializer.Add(ref pairs, (ISerializer s, ref (int, int) value) =>
            {
                value = (s.Reader.ReadInt(), s.Reader.ReadInt());
            });

            Assert.IsNull(nullArray);
            Assert.That(zeroArray, Is.Empty);
            Assert.That(smallArray, Is.EqualTo(new[] { 1, 2, 3}));
            Assert.That(stringArray, Is.EqualTo(new[] { "hello", "world" }));
            Assert.That(pairs, Is.EqualTo(new[] { (1, 2), (3, 4) }));
        }
        
        [Test]
        public void TestList()
        {
            ReaderWriterStream stream = new ReaderWriterStream();

            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, new TypenameBasedTypeSerializer());
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(stream, new TypenameBasedTypeDeserializer());

            List<int>? nullArray = null;
            List<int>? zeroArray = new List<int>();
            List<int>? smallArray = new List<int> { 1, 2, 3 };
            List<string?>? stringArray = new List<string?> { "hello", "world" };
            List<(int, int)>? pairs = new List<(int, int)> { (1, 2), (3, 4) };

            serializer.Add(ref nullArray);
            serializer.Add(ref zeroArray);
            serializer.Add(ref smallArray);
            serializer.Add(ref stringArray);
            serializer.Add(ref pairs, (ISerializer s, ref (int, int) value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });

            nullArray = zeroArray = smallArray = null;
            stringArray = null;
            pairs = null;

            deserializer.Add(ref nullArray);
            deserializer.Add(ref zeroArray);
            deserializer.Add(ref smallArray);
            deserializer.Add(ref stringArray);
            deserializer.Add(ref pairs, (ISerializer s, ref (int, int) value) =>
            {
                value = (s.Reader.ReadInt(), s.Reader.ReadInt());
            });

            Assert.IsNull(nullArray);
            Assert.That(zeroArray, Is.Empty);
            Assert.That(smallArray, Is.EqualTo(new[] { 1, 2, 3}));
            Assert.That(stringArray, Is.EqualTo(new[] { "hello", "world" }));
            Assert.That(pairs, Is.EqualTo(new[] { (1, 2), (3, 4) }));
        }
    }
}