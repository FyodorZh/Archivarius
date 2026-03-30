using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_List
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
        public void PrimitiveValueType_NullList_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            List<int>? list = null;
            s.AddList(ref list);

            list = new List<int> { 1 };
            d.AddList(ref list);

            Assert.IsNull(list);
        }

        [Test]
        public void PrimitiveValueType_EmptyList_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            List<int>? list = new List<int>();
            s.AddList(ref list);

            list = null;
            d.AddList(ref list);

            Assert.That(list, Is.Empty);
        }

        [Test]
        public void PrimitiveValueType_ListWithValues_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            List<int>? list = new List<int> { 10, 20, 30 };
            s.AddList(ref list);

            list = null;
            d.AddList(ref list);

            Assert.That(list, Is.EqualTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void PrimitiveValueType_NonNullableList_WhenListIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            List<int> list = new List<int> { 5, 6, 7 };
            s.AddList(ref list, () => throw new Exception("default factory must not be called"));

            list = null!;
            d.AddList(ref list, () => throw new Exception("default factory must not be called"));

            Assert.That(list, Is.EqualTo(new[] { 5, 6, 7 }));
        }

        [Test]
        public void PrimitiveValueType_NonNullableList_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            List<int>? nullableList = null;
            s.AddList(ref nullableList);

            List<int> list = new List<int>();
            d.AddList(ref list, () => new List<int> { 42 });

            Assert.That(list, Is.EqualTo(new[] { 42 }));
        }

        [Test]
        public void PrimitiveValueType_ListWithSingleElement_RoundTripsCorrectly()
        {
            var (s, d) = CreatePair();

            List<int>? list = new List<int> { 99 };
            s.AddList(ref list);

            list = null;
            d.AddList(ref list);

            Assert.That(list, Is.EqualTo(new[] { 99 }));
        }

        #endregion

        #region Primitive class type overloads

        [Test]
        public void PrimitiveClassType_NullList_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            List<string>? list = null;
            s.AddList(ref list, () => throw new Exception("element default must not be called"));

            list = new List<string> { "x" };
            d.AddList(ref list, () => throw new Exception("element default must not be called"));

            Assert.IsNull(list);
        }

        [Test]
        public void PrimitiveClassType_EmptyList_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            List<string>? list = new List<string>();
            s.AddList(ref list, () => throw new Exception("element default must not be called"));

            list = null;
            d.AddList(ref list, () => throw new Exception("element default must not be called"));

            Assert.That(list, Is.Empty);
        }

        [Test]
        public void PrimitiveClassType_ListWithNonNullElements_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            List<string>? list = new List<string> { "hello", "world" };
            s.AddList(ref list, () => throw new Exception("element default must not be called"));

            list = null;
            d.AddList(ref list, () => throw new Exception("element default must not be called"));

            Assert.That(list, Is.EqualTo(new[] { "hello", "world" }));
        }

        [Test]
        public void PrimitiveClassType_ListWithNullElements_NullElementsAreReplacedByElementDefault()
        {
            var (s, d) = CreatePair();

            List<string>? list = new List<string> { "a", null!, "b" };
            s.AddList(ref list, () => throw new Exception());

            list = null;
            d.AddList(ref list, () => "default");

            Assert.That(list, Is.EqualTo(new[] { "a", "default", "b" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableList_WhenListAndElementsArePresent_NeitherDefaultIsInvoked()
        {
            var (s, d) = CreatePair();

            List<string> list = new List<string> { "foo", "bar" };
            s.AddList(ref list, () => throw new Exception("list default must not be called"), () => throw new Exception("element default must not be called"));

            list = null!;
            d.AddList(ref list, () => throw new Exception("list default must not be called"), () => throw new Exception("element default must not be called"));

            Assert.That(list, Is.EqualTo(new[] { "foo", "bar" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableList_WhenNullWasSerialized_DefaultListValueIsUsed()
        {
            var (s, d) = CreatePair();

            List<string?>? nullableList = null;
            s.AddList(ref nullableList);

            List<string> list = new List<string>();
            d.AddList(ref list, () => new List<string> { "fallback" }, () => throw new Exception());

            Assert.That(list, Is.EqualTo(new[] { "fallback" }));
        }

        [Test]
        public void PrimitiveClassType_NonNullableList_WhenNullElementsPresent_ElementDefaultIsUsed()
        {
            var (s, d) = CreatePair();

            List<string> list = new List<string> { "x", null!, "z" };
            s.AddList(ref list, () => throw new Exception("list default must not be called"), () => throw new Exception());

            list = null!;
            d.AddList(ref list, () => throw new Exception("list default must not be called"), () => "replaced");

            Assert.That(list, Is.EqualTo(new[] { "x", "replaced", "z" }));
        }

        #endregion

        #region Non-primitive type overloads

        [Test]
        public void NonPrimitive_NullList_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            List<Tuple<int, int>>? list = null;
            s.AddList(ref list, SerializeTuple);

            list = new List<Tuple<int, int>> { new Tuple<int, int>(1, 1) };
            d.AddList(ref list, SerializeTuple);

            Assert.IsNull(list);
        }

        [Test]
        public void NonPrimitive_EmptyList_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            List<Tuple<int, int>>? list = new List<Tuple<int, int>>();
            s.AddList(ref list, SerializeTuple);

            list = null;
            d.AddList(ref list, SerializeTuple);

            Assert.That(list, Is.Empty);
        }

        [Test]
        public void NonPrimitive_ListWithValues_AllElementsArePreserved()
        {
            var (s, d) = CreatePair();

            List<Tuple<int, int>>? list = new List<Tuple<int, int>> { new Tuple<int, int>(1, 2), new Tuple<int, int>(3, 4) };
            s.AddList(ref list, SerializeTuple);

            list = null;
            d.AddList(ref list, SerializeTuple);

            Assert.That(list, Is.EqualTo(new[] { new Tuple<int, int>(1, 2), new Tuple<int, int>(3, 4) }));
        }

        [Test]
        public void NonPrimitive_NonNullableList_WhenListIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            List<Tuple<int, int>> list = new List<Tuple<int, int>> { new Tuple<int, int>(7, 8) };
            s.AddList(ref list, () => throw new Exception("default factory must not be called"), SerializeTuple);

            list = null!;
            d.AddList(ref list, () => throw new Exception("default factory must not be called"), SerializeTuple);

            Assert.That(list, Is.EqualTo(new[] { new Tuple<int, int>(7, 8) }));
        }

        [Test]
        public void NonPrimitive_NonNullableList_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            List<Tuple<int, int>>? nullableList = null;
            s.AddList(ref nullableList, SerializeTuple);

            var expected = new List<Tuple<int, int>> { new Tuple<int, int>(99, 99) };

            List<Tuple<int, int>> list = new List<Tuple<int, int>>();
            d.AddList(ref list, () => expected, SerializeTuple);

            Assert.That(list, Is.SameAs(expected));
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

