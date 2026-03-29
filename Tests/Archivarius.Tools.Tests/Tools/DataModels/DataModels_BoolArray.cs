using System;
using System.Collections.Generic;
using System.Linq;
using Archivarius.DataModels.Compressed;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class DataModels_BoolArray
    {
        [Test]
        public void EmptyArray_HasCountZero()
        {
            var boolArray = new BoolArray(Array.Empty<bool>());
            Assert.That(boolArray.Count, Is.EqualTo(0));
        }

        [Test]
        public void EmptyArray_ToArray_ReturnsEmpty()
        {
            var boolArray = new BoolArray(Array.Empty<bool>());
            Assert.That(boolArray.ToArray(), Is.EqualTo(Array.Empty<bool>()));
        }

        [Test]
        public void SingleTrue_RoundTrips()
        {
            var input = new[] { true };
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(1));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void SingleFalse_RoundTrips()
        {
            var input = new[] { false };
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(1));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void SevenElements_RoundTrips()
        {
            var input = new[] { true, false, true, true, false, false, true };
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(7));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void EightElements_CountIsCorrect()
        {
            var input = new[] { true, false, true, false, true, false, true, false };
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(8));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void NineElements_RoundTrips()
        {
            var input = new[] { true, false, true, false, true, false, true, false, true };
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(9));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void SixteenElements_CountIsCorrect()
        {
            var input = Enumerable.Repeat(true, 16).ToArray();
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(16));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void AllFalse_RoundTrips([Values(1, 7, 8, 9, 15, 16, 17, 100)] int count)
        {
            var input = new bool[count];
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(count));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void AllTrue_RoundTrips([Values(1, 7, 8, 9, 15, 16, 17, 100)] int count)
        {
            var input = Enumerable.Repeat(true, count).ToArray();
            var boolArray = new BoolArray(input);
            Assert.That(boolArray.Count, Is.EqualTo(count));
            Assert.That(boolArray.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Indexer_Get_ReturnsCorrectValues()
        {
            var input = new[] { true, false, true, false, true };
            var boolArray = new BoolArray(input);
            for (int i = 0; i < input.Length; i++)
            {
                Assert.That(boolArray[i], Is.EqualTo(input[i]));
            }
        }

        [Test]
        public void Indexer_Set_ModifiesValues()
        {
            var input = new[] { true, false, true, false, true };
            var boolArray = new BoolArray(input);

            boolArray[1] = true;
            Assert.That(boolArray[1], Is.True);

            boolArray[0] = false;
            Assert.That(boolArray[0], Is.False);

            Assert.That(boolArray.ToArray(), Is.EqualTo(new[] { false, true, true, false, true }));
        }

        [Test]
        public void Indexer_SetSameValue_DoesNotCorruptOtherBits()
        {
            var input = new[] { true, true, true, true, true, true, true, true };
            var boolArray = new BoolArray(input);

            boolArray[3] = false;

            Assert.That(boolArray[2], Is.True);
            Assert.That(boolArray[3], Is.False);
            Assert.That(boolArray[4], Is.True);
        }

        [Test]
        public void DefaultConstructor_HasCountZero()
        {
            var boolArray = new BoolArray();
            Assert.That(boolArray.Count, Is.EqualTo(0));
        }

        [Test]
        public void Serialize_EmptyArray_RoundTrips()
        {
            var original = new BoolArray(Array.Empty<bool>());
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(0));
            Assert.That(restored.ToArray(), Is.EqualTo(original.ToArray()));
        }

        [Test]
        public void Serialize_SmallArray_RoundTrips()
        {
            var input = new[] { true, false, true };
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(input.Length));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_ExactlyEightElements_RoundTrips()
        {
            var input = new[] { true, false, true, false, true, false, true, false };
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(8));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_ExactlySixteenElements_RoundTrips()
        {
            var input = Enumerable.Range(0, 16).Select(i => i % 3 == 0).ToArray();
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(16));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_NonMultipleOfEight_RoundTrips([Values(1, 3, 7, 9, 13, 15, 17, 31, 33)] int count)
        {
            var input = Enumerable.Range(0, count).Select(i => i % 2 == 0).ToArray();
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(count));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_MultipleOfEight_RoundTrips([Values(8, 16, 24, 32, 64)] int count)
        {
            var input = Enumerable.Range(0, count).Select(i => i % 2 == 0).ToArray();
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(count));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_LargeArray_RoundTrips()
        {
            var random = new Random(42);
            var input = Enumerable.Range(0, 1000).Select(_ => random.Next(2) == 1).ToArray();
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.Count, Is.EqualTo(input.Length));
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_AllTrue_RoundTrips()
        {
            var input = Enumerable.Repeat(true, 24).ToArray();
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        [Test]
        public void Serialize_AllFalse_RoundTrips()
        {
            var input = new bool[24];
            var original = new BoolArray(input);
            var restored = SerializeAndDeserialize(original);
            Assert.That(restored.ToArray(), Is.EqualTo(input));
        }

        private static BoolArray SerializeAndDeserialize(BoolArray original)
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);

            BoolArray? tmp = original;
            serializer.AddStaticClass(ref tmp);

            BoolArray? tmp2 = new BoolArray();
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();
            deserializer.AddStaticClass(ref tmp2);

            return tmp2!;
        }
    }
}