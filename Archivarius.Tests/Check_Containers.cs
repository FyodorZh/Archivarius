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
            int[] notNullArray = { 1 };
            string?[]? nullableStringArray = new string?[] { "hello", null };
            string[]? stringArray = new string[] { "hello", "world", null! };
            Tuple<int, int>?[]? nullableClassesArray = new Tuple<int, int>?[] { new (1, 2), null, new (3, 4) };
            Tuple<int, int>[]? classesArray = new Tuple<int, int>[] { new (1, 2), new (3, 4) };
            (int, int)[]? structArray = new (int, int)[] { (1, 2), (3, 4) };

            serializer.AddArray(ref nullArray);
            serializer.AddArray(ref zeroArray);
            serializer.AddArray(ref smallArray);
            serializer.AddArray(ref notNullArray, () => throw new Exception());
            serializer.AddArray(ref nullableStringArray);
            serializer.AddArray(ref stringArray, () => throw new Exception());
            serializer.AddArray(ref nullableClassesArray, (ISerializer s, ref Tuple<int, int>? value) =>
            {
                s.Writer.WriteBool(value != null);
                if (value != null)
                {
                    s.Writer.WriteInt(value.Item1);
                    s.Writer.WriteInt(value.Item2);
                }
            });
            serializer.AddArray(ref classesArray, (ISerializer s, ref Tuple<int, int> value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });
            serializer.AddArray(ref structArray, (ISerializer s, ref (int, int) value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });

            nullArray = zeroArray = smallArray = null;
            notNullArray = null!;
            nullableStringArray = null;
            stringArray = null;
            nullableClassesArray = null;
            classesArray = null;
            structArray = null;

            deserializer.AddArray(ref nullArray);
            deserializer.AddArray(ref zeroArray);
            deserializer.AddArray(ref smallArray);
            deserializer.AddArray(ref notNullArray, () => throw new Exception());
            deserializer.AddArray(ref nullableStringArray);
            deserializer.AddArray(ref stringArray, () => "!");
            deserializer.AddArray(ref nullableClassesArray, (ISerializer s, ref Tuple<int, int>? value) =>
            {
                if (s.Reader.ReadBool())
                {
                    value = new Tuple<int, int>(s.Reader.ReadInt(), s.Reader.ReadInt());
                }
                else
                {
                    value = null;
                }
            } );
            deserializer.AddArray(ref classesArray, (ISerializer s, ref Tuple<int, int> value) =>
            {
                value = new Tuple<int, int>(s.Reader.ReadInt(), s.Reader.ReadInt());
            });
            deserializer.AddArray(ref structArray, (ISerializer s, ref (int, int) value) =>
            {
                value = (s.Reader.ReadInt(), s.Reader.ReadInt());
            });

            Assert.IsNull(nullArray);
            Assert.That(zeroArray, Is.Empty);
            Assert.That(smallArray, Is.EqualTo(new[] { 1, 2, 3}));
            Assert.That(notNullArray, Is.EqualTo(new[] { 1 }));
            Assert.That(nullableStringArray, Is.EqualTo(new[] { "hello", null }));
            Assert.That(stringArray, Is.EqualTo(new[] { "hello", "world", "!" }));
            Assert.That(nullableClassesArray, Is.EqualTo(new Tuple<int, int>?[] { new (1, 2), null, new (3, 4) }));
            Assert.That(classesArray, Is.EqualTo(new Tuple<int, int>[] { new (1, 2), new (3, 4) }));
            Assert.That(structArray, Is.EqualTo(new[] { (1, 2), (3, 4) }));
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
            List<int> notNullArray = new List<int> { 1 };
            List<string?>? nullableStringArray = new List<string?> { "hello", null };
            List<string>? stringArray = new List<string> { "hello", "world", null! };
            List<Tuple<int, int>?>? nullableClassesArray = new List<Tuple<int, int>?> { new (1, 2), null, new (3, 4) };
            List<Tuple<int, int>>? classesArray = new List<Tuple<int, int>> { new (1, 2), new (3, 4) };
            List<(int, int)>? structArray = new List<(int, int)> { (1, 2), (3, 4) };
            

            serializer.AddList(ref nullArray);
            serializer.AddList(ref zeroArray);
            serializer.AddList(ref smallArray);
            serializer.AddList(ref notNullArray, () => throw new Exception());
            serializer.AddList(ref nullableStringArray);
            serializer.AddList(ref stringArray, () => throw new Exception());
            serializer.AddList(ref nullableClassesArray, (ISerializer s, ref Tuple<int, int>? value) => 
            {
                if (value == null)
                {
                    s.Writer.WriteBool(false);
                }
                else
                {
                    s.Writer.WriteBool(true);
                    s.Writer.WriteInt(value.Item1);
                    s.Writer.WriteInt(value.Item2);
                }
            });
            serializer.AddList(ref classesArray, (ISerializer s, ref Tuple<int, int> value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });
            serializer.AddList(ref structArray, (ISerializer s, ref (int, int) value) =>
            {
                s.Writer.WriteInt(value.Item1);
                s.Writer.WriteInt(value.Item2);
            });

            nullArray = zeroArray = smallArray = null;
            notNullArray = null!;
            nullableStringArray = null;
            stringArray = null;
            nullableClassesArray = null;
            classesArray = null;
            structArray = null;

            deserializer.AddList(ref nullArray);
            deserializer.AddList(ref zeroArray);
            deserializer.AddList(ref smallArray);
            deserializer.AddList(ref notNullArray, () => throw new Exception());
            deserializer.AddList(ref nullableStringArray);
            deserializer.AddList(ref stringArray, () => "!");
            deserializer.AddList(ref nullableClassesArray, (ISerializer s, ref Tuple<int, int>? value) => 
            {
                if (s.Reader.ReadBool())
                {
                    value = new Tuple<int, int>(s.Reader.ReadInt(), s.Reader.ReadInt());
                }
                else
                {
                    value = null;
                }
            });
            deserializer.AddList(ref classesArray, (ISerializer s, ref Tuple<int, int> value) =>
            {
                value = new Tuple<int, int>(s.Reader.ReadInt(), s.Reader.ReadInt());
            });
            deserializer.AddList(ref structArray, (ISerializer s, ref (int, int) value) =>
            {
                value = (s.Reader.ReadInt(), s.Reader.ReadInt());
            });

            Assert.IsNull(nullArray);
            Assert.That(zeroArray, Is.Empty);
            Assert.That(smallArray, Is.EqualTo(new[] { 1, 2, 3}));
            Assert.That(notNullArray, Is.EqualTo(new[] { 1}));
            Assert.That(nullableStringArray, Is.EqualTo(new[] { "hello", null }));
            Assert.That(stringArray, Is.EqualTo(new[] { "hello", "world", "!" }));
            Assert.That(nullableClassesArray, Is.EqualTo(new Tuple<int, int>?[] { new (1, 2), null, new (3, 4) }));
            Assert.That(classesArray, Is.EqualTo(new Tuple<int, int>[] { new (1, 2), new (3, 4) }));
            Assert.That(structArray, Is.EqualTo(new[] { (1, 2), (3, 4) }));
        }
    }
}