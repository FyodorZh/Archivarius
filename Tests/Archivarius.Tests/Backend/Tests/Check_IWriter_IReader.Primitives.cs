using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ── Primitive round-trip helper ──────────────────────────────────────────

        private async Task RoundTrip<T>(Action<IWriter, T> write, Func<IReader, T> read, T value)
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            write(writer, value);
            await FlushFetch(writer, reader);
            Assert.That(read(reader), Is.EqualTo(value));
        }

        // ── Bool ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Bool_True()
            => RoundTrip((w, v) => w.WriteBool(v), r => r.ReadBool(), true);

        [Test, Category("Primitives")]
        public Task Primitives_Bool_False()
            => RoundTrip((w, v) => w.WriteBool(v), r => r.ReadBool(), false);

        // ── Byte ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MinValue()
            => RoundTrip((w, v) => w.WriteByte(v), r => r.ReadByte(), (byte)0);

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MidValue()
            => RoundTrip((w, v) => w.WriteByte(v), r => r.ReadByte(), (byte)128);

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MaxValue()
            => RoundTrip((w, v) => w.WriteByte(v), r => r.ReadByte(), byte.MaxValue);

        // ── SByte ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_SByte_MinValue()
            => RoundTrip((w, v) => w.WriteSByte(v), r => r.ReadSByte(), sbyte.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_NegativeOne()
            => RoundTrip((w, v) => w.WriteSByte(v), r => r.ReadSByte(), (sbyte)-1);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_Zero()
            => RoundTrip((w, v) => w.WriteSByte(v), r => r.ReadSByte(), (sbyte)0);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_MaxValue()
            => RoundTrip((w, v) => w.WriteSByte(v), r => r.ReadSByte(), sbyte.MaxValue);

        // ── Char ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Char_NullChar()
            => RoundTrip((w, v) => w.WriteChar(v), r => r.ReadChar(), '\0');

        [Test, Category("Primitives")]
        public Task Primitives_Char_AsciiLetter()
            => RoundTrip((w, v) => w.WriteChar(v), r => r.ReadChar(), 'A');

        [Test, Category("Primitives")]
        public Task Primitives_Char_HighUnicode()
            => RoundTrip((w, v) => w.WriteChar(v), r => r.ReadChar(), '\u4E2D'); // 中

        [Test, Category("Primitives")]
        public Task Primitives_Char_MaxValue()
            => RoundTrip((w, v) => w.WriteChar(v), r => r.ReadChar(), char.MaxValue);

        // ── Short ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Short_MinValue()
            => RoundTrip((w, v) => w.WriteShort(v), r => r.ReadShort(), short.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Short_NegativeOne()
            => RoundTrip((w, v) => w.WriteShort(v), r => r.ReadShort(), (short)-1);

        [Test, Category("Primitives")]
        public Task Primitives_Short_Zero()
            => RoundTrip((w, v) => w.WriteShort(v), r => r.ReadShort(), (short)0);

        [Test, Category("Primitives")]
        public Task Primitives_Short_MaxValue()
            => RoundTrip((w, v) => w.WriteShort(v), r => r.ReadShort(), short.MaxValue);

        // ── UShort ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MinValue()
            => RoundTrip((w, v) => w.WriteUShort(v), r => r.ReadUShort(), (ushort)0);

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MidValue()
            => RoundTrip((w, v) => w.WriteUShort(v), r => r.ReadUShort(), (ushort)32768);

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MaxValue()
            => RoundTrip((w, v) => w.WriteUShort(v), r => r.ReadUShort(), ushort.MaxValue);

        // ── Int ──────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Int_MinValue()
            => RoundTrip((w, v) => w.WriteInt(v), r => r.ReadInt(), int.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Int_NegativeOne()
            => RoundTrip((w, v) => w.WriteInt(v), r => r.ReadInt(), -1);

        [Test, Category("Primitives")]
        public Task Primitives_Int_Zero()
            => RoundTrip((w, v) => w.WriteInt(v), r => r.ReadInt(), 0);

        [Test, Category("Primitives")]
        public Task Primitives_Int_One()
            => RoundTrip((w, v) => w.WriteInt(v), r => r.ReadInt(), 1);

        [Test, Category("Primitives")]
        public Task Primitives_Int_MaxValue()
            => RoundTrip((w, v) => w.WriteInt(v), r => r.ReadInt(), int.MaxValue);

        // ── UInt ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MinValue()
            => RoundTrip((w, v) => w.WriteUInt(v), r => r.ReadUInt(), 0u);

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MidValue()
            => RoundTrip((w, v) => w.WriteUInt(v), r => r.ReadUInt(), 2147483648u);

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MaxValue()
            => RoundTrip((w, v) => w.WriteUInt(v), r => r.ReadUInt(), uint.MaxValue);

        // ── Long ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Long_MinValue()
            => RoundTrip((w, v) => w.WriteLong(v), r => r.ReadLong(), long.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Long_NegativeOne()
            => RoundTrip((w, v) => w.WriteLong(v), r => r.ReadLong(), -1L);

        [Test, Category("Primitives")]
        public Task Primitives_Long_Zero()
            => RoundTrip((w, v) => w.WriteLong(v), r => r.ReadLong(), 0L);

        [Test, Category("Primitives")]
        public Task Primitives_Long_MaxValue()
            => RoundTrip((w, v) => w.WriteLong(v), r => r.ReadLong(), long.MaxValue);

        // ── ULong ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MinValue()
            => RoundTrip((w, v) => w.WriteULong(v), r => r.ReadULong(), 0uL);

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MidValue()
            => RoundTrip((w, v) => w.WriteULong(v), r => r.ReadULong(), (ulong)long.MaxValue + 1uL);

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MaxValue()
            => RoundTrip((w, v) => w.WriteULong(v), r => r.ReadULong(), ulong.MaxValue);

        // ── Float ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Float_Zero()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), 0f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_One()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), 1f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_NegativeOne()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), -1f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_MinValue()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), float.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Float_MaxValue()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), float.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Float_Epsilon()
            => RoundTrip((w, v) => w.WriteFloat(v), r => r.ReadFloat(), float.Epsilon);

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NegativeZero()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteFloat(-0f);
            await FlushFetch(writer, reader);
            float result = reader.ReadFloat();
            // Verify the bit pattern is preserved (-0f ≠ +0f at the bit level)
            Assert.That(BitConverter.SingleToUInt32Bits(result),
                Is.EqualTo(BitConverter.SingleToUInt32Bits(-0f)),
                "Negative zero must be preserved bitwise.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NaN()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteFloat(float.NaN);
            await FlushFetch(writer, reader);
            Assert.That(float.IsNaN(reader.ReadFloat()), Is.True, "NaN must round-trip as NaN.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_PositiveInfinity()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteFloat(float.PositiveInfinity);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadFloat(), Is.EqualTo(float.PositiveInfinity));
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NegativeInfinity()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteFloat(float.NegativeInfinity);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadFloat(), Is.EqualTo(float.NegativeInfinity));
        }

        // ── Double ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Double_Zero()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), 0d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_One()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), 1d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_NegativeOne()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), -1d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_MinValue()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), double.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Double_MaxValue()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), double.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Double_Epsilon()
            => RoundTrip((w, v) => w.WriteDouble(v), r => r.ReadDouble(), double.Epsilon);

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NegativeZero()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteDouble(-0d);
            await FlushFetch(writer, reader);
            double result = reader.ReadDouble();
            Assert.That(BitConverter.DoubleToUInt64Bits(result),
                Is.EqualTo(BitConverter.DoubleToUInt64Bits(-0d)),
                "Negative zero must be preserved bitwise.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NaN()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteDouble(double.NaN);
            await FlushFetch(writer, reader);
            Assert.That(double.IsNaN(reader.ReadDouble()), Is.True, "NaN must round-trip as NaN.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_PositiveInfinity()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteDouble(double.PositiveInfinity);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadDouble(), Is.EqualTo(double.PositiveInfinity));
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NegativeInfinity()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteDouble(double.NegativeInfinity);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadDouble(), Is.EqualTo(double.NegativeInfinity));
        }

        // ── Decimal ──────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_Zero()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), 0m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_One()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), 1m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_NegativeOne()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), -1m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_MinValue()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), decimal.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_MaxValue()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), decimal.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_HighPrecision()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), 0.0000000000000000000000000001m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_Pi()
            => RoundTrip((w, v) => w.WriteDecimal(v), r => r.ReadDecimal(), 3.14159265358979323846m);

        // ── String ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_String_Null()
            => RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), (string?)null);

        [Test, Category("Primitives")]
        public Task Primitives_String_Empty()
            => RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), "");

        [Test, Category("Primitives")]
        public Task Primitives_String_Ascii()
            => RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), "Hello, world!");

        [Test, Category("Primitives")]
        public Task Primitives_String_Unicode()
            => RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), "Привет мир 🌍 中文 日本語");

        [Test, Category("Primitives")]
        public Task Primitives_String_EmbeddedNullChar()
            => RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), "a\0b");

        [Test, Category("Primitives")]
        public async Task Primitives_String_Long()
        {
            string longStr = new string('x', 10_000);
            await RoundTrip((w, v) => w.WriteString(v), r => r.ReadString(), longStr);
        }

        // ── Array ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Array_Null()
            => RoundTrip((w, v) => w.WriteArray(v), r => r.ReadArray(), (byte[]?)null);

        [Test, Category("Primitives")]
        public Task Primitives_Array_Empty()
            => RoundTrip((w, v) => w.WriteArray(v), r => r.ReadArray(), Array.Empty<byte>());

        [Test, Category("Primitives")]
        public Task Primitives_Array_SingleByte()
            => RoundTrip((w, v) => w.WriteArray(v), r => r.ReadArray(), new byte[] { 42 });

        [Test, Category("Primitives")]
        public Task Primitives_Array_MultipleBytes()
            => RoundTrip((w, v) => w.WriteArray(v), r => r.ReadArray(), new byte[] { 0, 1, 127, 128, 255 });

        [Test, Category("Primitives")]
        public async Task Primitives_Array_Large()
        {
            byte[] buf = new byte[64 * 1024];
            for (int i = 0; i < buf.Length; i++) buf[i] = (byte)(i % 256);
            await RoundTrip((w, v) => w.WriteArray(v), r => r.ReadArray(), buf);
        }

        // ── Multi-value sequences ─────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public async Task Primitives_AllTypes_Sequential_RoundTrip()
        {
            var (writer, reader, _) = await GetWriterReaderPair();

            writer.WriteBool(true);
            writer.WriteByte(0xFF);
            writer.WriteSByte(-42);
            writer.WriteChar('Z');
            writer.WriteShort(-1000);
            writer.WriteUShort(60000);
            writer.WriteInt(-100_000);
            writer.WriteUInt(4_000_000_000u);
            writer.WriteLong(long.MinValue);
            writer.WriteULong(ulong.MaxValue);
            writer.WriteFloat(3.14f);
            writer.WriteDouble(2.718281828d);
            writer.WriteDecimal(1.23456789012345678901234567m);
            writer.WriteString("hello");
            writer.WriteString(null);
            writer.WriteArray(new byte[] { 1, 2, 3 });
            writer.WriteArray(null);

            await FlushFetch(writer, reader);

            Assert.That(reader.ReadBool(),    Is.EqualTo(true));
            Assert.That(reader.ReadByte(),    Is.EqualTo((byte)0xFF));
            Assert.That(reader.ReadSByte(),   Is.EqualTo((sbyte)-42));
            Assert.That(reader.ReadChar(),    Is.EqualTo('Z'));
            Assert.That(reader.ReadShort(),   Is.EqualTo((short)-1000));
            Assert.That(reader.ReadUShort(),  Is.EqualTo((ushort)60000));
            Assert.That(reader.ReadInt(),     Is.EqualTo(-100_000));
            Assert.That(reader.ReadUInt(),    Is.EqualTo(4_000_000_000u));
            Assert.That(reader.ReadLong(),    Is.EqualTo(long.MinValue));
            Assert.That(reader.ReadULong(),   Is.EqualTo(ulong.MaxValue));
            Assert.That(reader.ReadFloat(),   Is.EqualTo(3.14f));
            Assert.That(reader.ReadDouble(),  Is.EqualTo(2.718281828d));
            Assert.That(reader.ReadDecimal(), Is.EqualTo(1.23456789012345678901234567m));
            Assert.That(reader.ReadString(),  Is.EqualTo("hello"));
            Assert.That(reader.ReadString(),  Is.Null);
            Assert.That(reader.ReadArray(),   Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(reader.ReadArray(),   Is.Null);
        }

        [Test, Category("Primitives")]
        public async Task Primitives_SameType_MultipleValues_Sequential()
        {
            // Write 100 ints in sequence to stress sequential consistency.
            var (writer, reader, _) = await GetWriterReaderPair();
            const int count = 100;
            for (int i = 0; i < count; i++)
                writer.WriteInt(i);
            await FlushFetch(writer, reader);
            for (int i = 0; i < count; i++)
                Assert.That(reader.ReadInt(), Is.EqualTo(i), $"Mismatch at index {i}");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_StringAndArrayInterleaved()
        {
            // Verify that null/non-null strings and arrays are correctly distinguished.
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteString("first");
            writer.WriteArray(new byte[] { 10, 20 });
            writer.WriteString(null);
            writer.WriteArray(Array.Empty<byte>());
            writer.WriteArray(null);
            writer.WriteString("");

            await FlushFetch(writer, reader);

            Assert.That(reader.ReadString(), Is.EqualTo("first"));
            Assert.That(reader.ReadArray(),  Is.EqualTo(new byte[] { 10, 20 }));
            Assert.That(reader.ReadString(), Is.Null);
            Assert.That(reader.ReadArray(),  Is.EqualTo(Array.Empty<byte>()));
            Assert.That(reader.ReadArray(),  Is.Null);
            Assert.That(reader.ReadString(), Is.EqualTo(""));
        }
    }
}

