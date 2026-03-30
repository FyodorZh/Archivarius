using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_Dictionary
    {
        private static (HierarchicalSerializer serializer, HierarchicalDeserializer deserializer) CreatePair()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            return (
                new HierarchicalSerializer(stream, false),
                HierarchicalDeserializer.From(stream).SetMonomorphic().Build()
            );
        }

        #region Primitive key and value type overloads

        [Test]
        public void PrimitiveTypes_NullDictionary_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int>? dict = null;
            s.AddDictionary(ref dict);

            dict = new Dictionary<int, int> { { 1, 2 } };
            d.AddDictionary(ref dict);

            Assert.IsNull(dict);
        }

        [Test]
        public void PrimitiveTypes_EmptyDictionary_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int>? dict = new Dictionary<int, int>();
            s.AddDictionary(ref dict);

            dict = null;
            d.AddDictionary(ref dict);

            Assert.That(dict, Is.Empty);
        }

        [Test]
        public void PrimitiveTypes_DictionaryWithEntries_AllEntriesArePreserved()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int>? dict = new Dictionary<int, int> { { 1, 10 }, { 2, 20 }, { 3, 30 } };
            s.AddDictionary(ref dict);

            dict = null;
            d.AddDictionary(ref dict);

            Assert.That(dict, Is.EqualTo(new Dictionary<int, int> { { 1, 10 }, { 2, 20 }, { 3, 30 } }));
        }

        [Test]
        public void PrimitiveTypes_NonNullableDictionary_WhenDictionaryIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int> dict = new Dictionary<int, int> { { 7, 77 } };
            s.AddDictionary(ref dict, () => throw new Exception("default factory must not be called"));

            dict = null!;
            d.AddDictionary(ref dict, () => throw new Exception("default factory must not be called"));

            Assert.That(dict, Is.EqualTo(new Dictionary<int, int> { { 7, 77 } }));
        }

        [Test]
        public void PrimitiveTypes_NonNullableDictionary_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int>? nullableDict = null;
            s.AddDictionary(ref nullableDict);

            var expected = new Dictionary<int, int> { { 42, 99 } };
            Dictionary<int, int> dict = new Dictionary<int, int>();
            d.AddDictionary(ref dict, () => expected);

            Assert.That(dict, Is.SameAs(expected));
        }

        [Test]
        public void PrimitiveTypes_DictionaryWithSingleEntry_RoundTripsCorrectly()
        {
            var (s, d) = CreatePair();

            Dictionary<int, int>? dict = new Dictionary<int, int> { { 5, 50 } };
            s.AddDictionary(ref dict);

            dict = null;
            d.AddDictionary(ref dict);

            Assert.That(dict, Is.EqualTo(new Dictionary<int, int> { { 5, 50 } }));
        }

        #endregion

        #region Non-primitive type overloads

        [Test]
        public void NonPrimitive_NullDictionary_RoundTripsAsNull()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>>? dict = null;
            s.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            dict = new Dictionary<string, Tuple<int, int>> { { "x", new Tuple<int, int>(1, 2) } };
            d.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            Assert.IsNull(dict);
        }

        [Test]
        public void NonPrimitive_EmptyDictionary_RoundTripsAsEmpty()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>>? dict = new Dictionary<string, Tuple<int, int>>();
            s.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            dict = null;
            d.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            Assert.That(dict, Is.Empty);
        }

        [Test]
        public void NonPrimitive_DictionaryWithEntries_AllEntriesArePreserved()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>>? dict = new Dictionary<string, Tuple<int, int>>
            {
                { "a", new Tuple<int, int>(1, 2) },
                { "b", new Tuple<int, int>(3, 4) },
            };
            s.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            dict = null;
            d.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            Assert.That(dict, Is.EqualTo(new Dictionary<string, Tuple<int, int>>
            {
                { "a", new Tuple<int, int>(1, 2) },
                { "b", new Tuple<int, int>(3, 4) },
            }));
        }

        [Test]
        public void NonPrimitive_NonNullableDictionary_WhenDictionaryIsPresent_DefaultFactoryIsNotInvoked()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>> dict = new Dictionary<string, Tuple<int, int>>
            {
                { "key", new Tuple<int, int>(9, 8) }
            };
            s.AddDictionary(ref dict, () => throw new Exception("default factory must not be called"), SerializeStringKey, SerializeTupleValue);

            dict = null!;
            d.AddDictionary(ref dict, () => throw new Exception("default factory must not be called"), SerializeStringKey, SerializeTupleValue);

            Assert.That(dict, Is.EqualTo(new Dictionary<string, Tuple<int, int>> { { "key", new Tuple<int, int>(9, 8) } }));
        }

        [Test]
        public void NonPrimitive_NonNullableDictionary_WhenNullWasSerialized_DefaultValueIsUsed()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>>? nullableDict = null;
            s.AddDictionary(ref nullableDict, SerializeStringKey, SerializeTupleValue);

            var expected = new Dictionary<string, Tuple<int, int>> { { "fallback", new Tuple<int, int>(0, 0) } };
            Dictionary<string, Tuple<int, int>> dict = new Dictionary<string, Tuple<int, int>>();
            d.AddDictionary(ref dict, () => expected, SerializeStringKey, SerializeTupleValue);

            Assert.That(dict, Is.SameAs(expected));
        }

        [Test]
        public void NonPrimitive_DictionaryWithMultipleEntries_KeysAndValuesMatchAfterRoundTrip()
        {
            var (s, d) = CreatePair();

            Dictionary<string, Tuple<int, int>>? dict = new Dictionary<string, Tuple<int, int>>
            {
                { "first", new Tuple<int, int>(10, 11) },
                { "second", new Tuple<int, int>(20, 21) },
                { "third", new Tuple<int, int>(30, 31) },
            };
            s.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            dict = null;
            d.AddDictionary(ref dict, SerializeStringKey, SerializeTupleValue);

            Assert.That(dict!["first"], Is.EqualTo(new Tuple<int, int>(10, 11)));
            Assert.That(dict!["second"], Is.EqualTo(new Tuple<int, int>(20, 21)));
            Assert.That(dict!["third"], Is.EqualTo(new Tuple<int, int>(30, 31)));
            Assert.That(dict!.Count, Is.EqualTo(3));
        }

        private static void SerializeStringKey(ISerializer serializer, ref string value)
        {
            if (serializer.IsWriter)
            {
                serializer.Writer.WriteString(value);
            }
            else
            {
                value = serializer.Reader.ReadString()!;
            }
        }

        private static void SerializeTupleValue(ISerializer serializer, ref Tuple<int, int> value)
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

