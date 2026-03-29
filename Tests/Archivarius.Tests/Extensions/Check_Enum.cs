using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_Enum
    {
        private enum ByteEnum : byte { Zero = 0, One = 1, Max = byte.MaxValue }
        private enum SByteEnum : sbyte { Min = sbyte.MinValue, Zero = 0, Max = sbyte.MaxValue }
        private enum ShortEnum : short { Min = short.MinValue, Zero = 0, Max = short.MaxValue }
        private enum UShortEnum : ushort { Zero = 0, One = 1, Max = ushort.MaxValue }
        private enum IntEnum : int { Min = int.MinValue, Zero = 0, Max = int.MaxValue }
        private enum UIntEnum : uint { Zero = 0, One = 1, Max = uint.MaxValue }
        private enum LongEnum : long { Min = long.MinValue, Zero = 0, Max = long.MaxValue }
        private enum ULongEnum : ulong { Zero = 0, One = 1, Max = ulong.MaxValue }

        [Test]
        public void ByteEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ByteEnum written = ByteEnum.Max;
            serializer.Add(ref written);

            ByteEnum read = ByteEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(ByteEnum.Max));
        }

        [Test]
        public void SByteEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            SByteEnum written = SByteEnum.Min;
            serializer.Add(ref written);

            SByteEnum read = SByteEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(SByteEnum.Min));
        }

        [Test]
        public void ShortEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ShortEnum written = ShortEnum.Min;
            serializer.Add(ref written);

            ShortEnum read = ShortEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(ShortEnum.Min));
        }

        [Test]
        public void UShortEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            UShortEnum written = UShortEnum.Max;
            serializer.Add(ref written);

            UShortEnum read = UShortEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(UShortEnum.Max));
        }

        [Test]
        public void IntEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            IntEnum written = IntEnum.Min;
            serializer.Add(ref written);

            IntEnum read = IntEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(IntEnum.Min));
        }

        [Test]
        public void UIntEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            UIntEnum written = UIntEnum.Max;
            serializer.Add(ref written);

            UIntEnum read = UIntEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(UIntEnum.Max));
        }

        [Test]
        public void LongEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            LongEnum written = LongEnum.Min;
            serializer.Add(ref written);

            LongEnum read = LongEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(LongEnum.Min));
        }

        [Test]
        public void ULongEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ULongEnum written = ULongEnum.Max;
            serializer.Add(ref written);

            ULongEnum read = ULongEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(ULongEnum.Max));
        }

        [Test]
        public void ByteEnum_Zero_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ByteEnum written = ByteEnum.Zero;
            serializer.Add(ref written);

            ByteEnum read = ByteEnum.Max;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(ByteEnum.Zero));
        }

        [Test]
        public void IntEnum_MaxValue_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            IntEnum written = IntEnum.Max;
            serializer.Add(ref written);

            IntEnum read = IntEnum.Zero;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(IntEnum.Max));
        }

        [Test]
        public void MultipleEnumValues_SerializedInOrder()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ByteEnum b1 = ByteEnum.Zero;
            ByteEnum b2 = ByteEnum.One;
            ByteEnum b3 = ByteEnum.Max;
            serializer.Add(ref b1);
            serializer.Add(ref b2);
            serializer.Add(ref b3);

            ByteEnum r1 = ByteEnum.Max;
            ByteEnum r2 = ByteEnum.Max;
            ByteEnum r3 = ByteEnum.Zero;
            deserializer.Add(ref r1);
            deserializer.Add(ref r2);
            deserializer.Add(ref r3);

            Assert.That(r1, Is.EqualTo(ByteEnum.Zero));
            Assert.That(r2, Is.EqualTo(ByteEnum.One));
            Assert.That(r3, Is.EqualTo(ByteEnum.Max));
        }

        [Test]
        public void EnumInsideDataStruct_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            DataWithEnums? written = new DataWithEnums
            {
                ByteVal = ByteEnum.Max,
                ShortVal = ShortEnum.Min,
                IntVal = IntEnum.Max,
                LongVal = LongEnum.Min,
                SByteVal = SByteEnum.Min,
                UShortVal = UShortEnum.Max,
                UIntVal = UIntEnum.Max,
                ULongVal = ULongEnum.Max
            };
            serializer.AddStaticClass(ref written);

            DataWithEnums? read = new DataWithEnums();
            deserializer.AddStaticClass(ref read);

            Assert.That(read!.ByteVal, Is.EqualTo(ByteEnum.Max));
            Assert.That(read.ShortVal, Is.EqualTo(ShortEnum.Min));
            Assert.That(read.IntVal, Is.EqualTo(IntEnum.Max));
            Assert.That(read.LongVal, Is.EqualTo(LongEnum.Min));
            Assert.That(read.SByteVal, Is.EqualTo(SByteEnum.Min));
            Assert.That(read.UShortVal, Is.EqualTo(UShortEnum.Max));
            Assert.That(read.UIntVal, Is.EqualTo(UIntEnum.Max));
            Assert.That(read.ULongVal, Is.EqualTo(ULongEnum.Max));
        }

        [Test]
        public void AllUnderlyingTypes_ZeroValues_RoundTrip()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ByteEnum byteVal = ByteEnum.Zero;
            SByteEnum sbyteVal = SByteEnum.Zero;
            ShortEnum shortVal = ShortEnum.Zero;
            UShortEnum ushortVal = UShortEnum.Zero;
            IntEnum intVal = IntEnum.Zero;
            UIntEnum uintVal = UIntEnum.Zero;
            LongEnum longVal = LongEnum.Zero;
            ULongEnum ulongVal = ULongEnum.Zero;

            serializer.Add(ref byteVal);
            serializer.Add(ref sbyteVal);
            serializer.Add(ref shortVal);
            serializer.Add(ref ushortVal);
            serializer.Add(ref intVal);
            serializer.Add(ref uintVal);
            serializer.Add(ref longVal);
            serializer.Add(ref ulongVal);

            byteVal = ByteEnum.Max;
            sbyteVal = SByteEnum.Max;
            shortVal = ShortEnum.Max;
            ushortVal = UShortEnum.Max;
            intVal = IntEnum.Max;
            uintVal = UIntEnum.Max;
            longVal = LongEnum.Max;
            ulongVal = ULongEnum.Max;

            deserializer.Add(ref byteVal);
            deserializer.Add(ref sbyteVal);
            deserializer.Add(ref shortVal);
            deserializer.Add(ref ushortVal);
            deserializer.Add(ref intVal);
            deserializer.Add(ref uintVal);
            deserializer.Add(ref longVal);
            deserializer.Add(ref ulongVal);

            Assert.That(byteVal, Is.EqualTo(ByteEnum.Zero));
            Assert.That(sbyteVal, Is.EqualTo(SByteEnum.Zero));
            Assert.That(shortVal, Is.EqualTo(ShortEnum.Zero));
            Assert.That(ushortVal, Is.EqualTo(UShortEnum.Zero));
            Assert.That(intVal, Is.EqualTo(IntEnum.Zero));
            Assert.That(uintVal, Is.EqualTo(UIntEnum.Zero));
            Assert.That(longVal, Is.EqualTo(LongEnum.Zero));
            Assert.That(ulongVal, Is.EqualTo(ULongEnum.Zero));
        }

        [Test]
        public void ShortEnum_NegativeValue_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            ShortEnum written = ShortEnum.Min;
            serializer.Add(ref written);

            ShortEnum read = ShortEnum.Max;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(ShortEnum.Min));
        }

        [Test]
        public void IntEnum_NegativeValue_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            IntEnum written = IntEnum.Min;
            serializer.Add(ref written);

            IntEnum read = IntEnum.Max;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(IntEnum.Min));
        }

        [Test]
        public void LongEnum_NegativeValue_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            LongEnum written = LongEnum.Min;
            serializer.Add(ref written);

            LongEnum read = LongEnum.Max;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(LongEnum.Min));
        }

        [Test]
        public void DefaultIntEnum_RoundTrips()
        {
            ReaderWriterStream stream = new ReaderWriterStream();
            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, false);
            HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(stream).SetMonomorphic().Build();

            DefaultIntEnum written = DefaultIntEnum.Second;
            serializer.Add(ref written);

            DefaultIntEnum read = DefaultIntEnum.First;
            deserializer.Add(ref read);

            Assert.That(read, Is.EqualTo(DefaultIntEnum.Second));
        }

        private enum DefaultIntEnum { First, Second, Third }

        private class DataWithEnums : IDataStruct
        {
            public ByteEnum ByteVal;
            public ShortEnum ShortVal;
            public IntEnum IntVal;
            public LongEnum LongVal;
            public SByteEnum SByteVal;
            public UShortEnum UShortVal;
            public UIntEnum UIntVal;
            public ULongEnum ULongVal;

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref ByteVal);
                serializer.Add(ref ShortVal);
                serializer.Add(ref IntVal);
                serializer.Add(ref LongVal);
                serializer.Add(ref SByteVal);
                serializer.Add(ref UShortVal);
                serializer.Add(ref UIntVal);
                serializer.Add(ref ULongVal);
            }
        }
    }
}

