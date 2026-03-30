using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_Array
    {
        private static (HierarchicalSerializer serializer, HierarchicalDeserializer deserializer) CreatePair()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            return (
                new HierarchicalSerializer(stream, false),
                HierarchicalDeserializer.From(stream).SetMonomorphic().Build()
            );
        }

        #region Primitive value type overloads

        [Test]
        public void PrimitiveValueType_NullArray_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            int[]? array = null;
            s.AddArray(ref array);

            array = new[] { 1 };
            d.AddArray(ref array);

            Assert.IsNull(array);
        }

        [Test]
        public void PrimitiveValueType_EmptyArray_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            int[]? array = Array.Empty<int>();
            s.AddArray(ref array);

            array = null;
            d.AddArray(ref array);

            Assert.That(array, Is.Empty);
        }

        [Test]
        public void PrimitiveValueType_ArrayWithValues_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            int[]? array = new[] { 10, 20, 30 };
            s.AddArray(ref array);

            array = null;
            d.AddArray(ref array);

            Assert.That(array, Is.EqualTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void PrimitiveValueType_NonNullableArray_WhenArrayIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            int[] array = new[] { 5, 6, 7 };
            s.AddArray(ref array, () => throw new Exception("default factory must not be called"));

            array = null!;
            d.AddArray(ref array, () => throw new Exception("default factory must not be called"));

            Assert.That(array, Is.EqualTo(new[] { 5, 6, 7 }));
        }

        [Test]
        public void PrimitiveValueType_NonNullableArray_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            int[]? nullableArray = null;
            s.AddArray(ref nullableArray);

            int[] array = Array.Empty<int>();
            d.AddArray(ref array, () => new[] { 42 });

            Assert.That(array, Is.EqualTo(new[] { 42 }));
        }

        #endregion

        #region Primitive class type overloads

        [Test]
        public void PrimitiveClassType_NullArray_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            string[]? array = null;
            s.AddArray(ref array, () => throw new Exception("element default must not be called"));

            array = new[] { "x" };
            d.AddArray(ref array, () => throw new Exception("element default must not be called"));

            Assert.IsNull(array);
        }

        [Test]
        public void PrimitiveClassType_EmptyArray_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            string[]? array = Array.Empty<string>();
            s.AddArray(ref array, () => throw new Exception("element default must not be called"));

            array = null;
            d.AddArray(ref array, () => throw new Exception("element default must not be called"));

            Assert.That(array, Is.Empty);
        }

        [Test]
        public void PrimitiveClassType_ArrayWithNonNullElements_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            string[]? array = new[] { "hello", "world" };
            s.AddArray(ref array, () => throw new Exception("element default must not be called"));

            array = null;
            d.AddArray(ref array, () => throw new Exception("element default must not be called"));

            Assert.That(array, Is.EqualTo(new[] { "hello", "world" }));
        }

        [Test]
        public void PrimitiveClassType_ArrayWithNullElements_NullElementsAreReplacedByElementDefault()
        {
            var (s, d) = CreatePair();

            string[]? array = new string[] { "a", null!, "b" };
            s.AddArray(ref array, () => throw new Exception());

            array = null;
            d.AddArray(ref array, () => "default");

            Assert.That(array, Is.EqualTo(new[] { "a", "default", "b" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableArray_WhenArrayAndElementsArePresent_NeitherDefaultIsInvoked()
        {
            var (s, d) = CreatePair();

            string[] array = new[] { "foo", "bar" };
            s.AddArray(ref array, () => throw new Exception("array default must not be called"), () => throw new Exception("element default must not be called"));

            array = null!;
            d.AddArray(ref array, () => throw new Exception("array default must not be called"), () => throw new Exception("element default must not be called"));

            Assert.That(array, Is.EqualTo(new[] { "foo", "bar" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableArray_WhenNullWasSerialized_DefaultArrayValueIsUsed()
        {
            var (s, d) = CreatePair();

            string?[]? nullableArray = null;
            s.AddArray(ref nullableArray);

            string[] array = Array.Empty<string>();
            d.AddArray(ref array, () => new[] { "fallback" }, () => throw new Exception());

            Assert.That(array, Is.EqualTo(new[] { "fallback" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableArray_WhenNullElementsPresent_ElementDefaultIsUsed()
        {
            var (s, d) = CreatePair();

            string[] array = new[] { "x", null!, "z" };
            s.AddArray(ref array, () => throw new Exception("array default must not be called"), () => throw new Exception());

            array = null!;
            d.AddArray(ref array, () => throw new Exception("array default must not be called"), () => "replaced");

            Assert.That(array, Is.EqualTo(new[] { "x", "replaced", "z" }));
        }

        #endregion

        #region Non-primitive type overloads

        [Test]
        public void NonPrimitive_NullArray_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            Tuple<int, int>[]? array = null;
            s.AddArray(ref array, SerializeTuple);

            array = new[] { new Tuple<int, int>(1, 1) };
            d.AddArray(ref array, SerializeTuple);

            Assert.IsNull(array);
        }

        [Test]
        public void NonPrimitive_EmptyArray_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            Tuple<int, int>[]? array = Array.Empty<Tuple<int, int>>();
            s.AddArray(ref array, SerializeTuple);

            array = null;
            d.AddArray(ref array, SerializeTuple);

            Assert.That(array, Is.Empty);
        }

        [Test]
        public void NonPrimitive_ArrayWithValues_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            Tuple<int, int>[]? array = new[] { new Tuple<int, int>(1, 2), new Tuple<int, int>(3, 4) };
            s.AddArray(ref array, SerializeTuple);

            array = null;
            d.AddArray(ref array, SerializeTuple);

            Assert.That(array, Is.EqualTo(new[] { new Tuple<int, int>(1, 2), new Tuple<int, int>(3, 4) }));
        }

        [Test]
        public void NonPrimitive_NonNullableArray_WhenArrayIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            Tuple<int, int>[] array = new[] { new Tuple<int, int>(7, 8) };
            s.AddArray(ref array, () => throw new Exception("default factory must not be called"), SerializeTuple);

            array = null!;
            d.AddArray(ref array, () => throw new Exception("default factory must not be called"), SerializeTuple);

            Assert.That(array, Is.EqualTo(new[] { new Tuple<int, int>(7, 8) }));
        }

        [Test]
        public void NonPrimitive_NonNullableArray_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            Tuple<int, int>[]? nullableArray = null;
            s.AddArray(ref nullableArray, SerializeTuple);

            var expected = new[] { new Tuple<int, int>(99, 99) };

            Tuple<int, int>[] array = Array.Empty<Tuple<int, int>>();
            d.AddArray(ref array, () => expected, SerializeTuple);

            Assert.That(array, Is.SameAs(expected));
        }

        private static void SerializeTuple(ISerializer serializer, ref Tuple<int, int> value)
        {
            if (serializer.IsWriter)
            {
                serializer.Writer.WriteInt(value.Item1);
                serializer.Writer.WriteInt(value.Item2);
            }
            else
            {
                value = new Tuple<int, int>(serializer.Reader.ReadInt(), serializer.Reader.ReadInt());
            }
        }

        #endregion
    }
}

