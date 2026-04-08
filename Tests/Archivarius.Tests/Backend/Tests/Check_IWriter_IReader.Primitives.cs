using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ── Primitive round-trip helper ──────────────────────────────────────────

        private async Task RoundTrip<T>(Action<IWriterBackend, T> write, Func<IReaderBackend, T> read, T value)
        {
            var (writer, reader) = await GetWriterReaderPair();
            write(writer, value);
            await CompleteFetch(writer, reader);
            Assert.That(read(reader), Is.EqualTo(value));
        }

        // ── Bool ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Bool_True()
            => RoundTrip((w, v) => w.Writer.WriteBool(v), r => r.Reader.ReadBool(), true);

        [Test, Category("Primitives")]
        public Task Primitives_Bool_False()
            => RoundTrip((w, v) => w.Writer.WriteBool(v), r => r.Reader.ReadBool(), false);

        // ── Byte ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteByte(v), r => r.Reader.ReadByte(), (byte)0);

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MidValue()
            => RoundTrip((w, v) => w.Writer.WriteByte(v), r => r.Reader.ReadByte(), (byte)128);

        [Test, Category("Primitives")]
        public Task Primitives_Byte_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteByte(v), r => r.Reader.ReadByte(), byte.MaxValue);

        // ── SByte ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_SByte_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteSByte(v), r => r.Reader.ReadSByte(), sbyte.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteSByte(v), r => r.Reader.ReadSByte(), (sbyte)-1);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_Zero()
            => RoundTrip((w, v) => w.Writer.WriteSByte(v), r => r.Reader.ReadSByte(), (sbyte)0);

        [Test, Category("Primitives")]
        public Task Primitives_SByte_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteSByte(v), r => r.Reader.ReadSByte(), sbyte.MaxValue);

        // ── Char ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Char_NullChar()
            => RoundTrip((w, v) => w.Writer.WriteChar(v), r => r.Reader.ReadChar(), '\0');

        [Test, Category("Primitives")]
        public Task Primitives_Char_AsciiLetter()
            => RoundTrip((w, v) => w.Writer.WriteChar(v), r => r.Reader.ReadChar(), 'A');

        [Test, Category("Primitives")]
        public Task Primitives_Char_HighUnicode()
            => RoundTrip((w, v) => w.Writer.WriteChar(v), r => r.Reader.ReadChar(), '\u4E2D'); // 中

        [Test, Category("Primitives")]
        public Task Primitives_Char_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteChar(v), r => r.Reader.ReadChar(), char.MaxValue);

        // ── Short ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Short_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteShort(v), r => r.Reader.ReadShort(), short.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Short_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteShort(v), r => r.Reader.ReadShort(), (short)-1);

        [Test, Category("Primitives")]
        public Task Primitives_Short_Zero()
            => RoundTrip((w, v) => w.Writer.WriteShort(v), r => r.Reader.ReadShort(), (short)0);

        [Test, Category("Primitives")]
        public Task Primitives_Short_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteShort(v), r => r.Reader.ReadShort(), short.MaxValue);

        // ── UShort ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteUShort(v), r => r.Reader.ReadUShort(), (ushort)0);

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MidValue()
            => RoundTrip((w, v) => w.Writer.WriteUShort(v), r => r.Reader.ReadUShort(), (ushort)32768);

        [Test, Category("Primitives")]
        public Task Primitives_UShort_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteUShort(v), r => r.Reader.ReadUShort(), ushort.MaxValue);

        // ── Int ──────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Int_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteInt(v), r => r.Reader.ReadInt(), int.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Int_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteInt(v), r => r.Reader.ReadInt(), -1);

        [Test, Category("Primitives")]
        public Task Primitives_Int_Zero()
            => RoundTrip((w, v) => w.Writer.WriteInt(v), r => r.Reader.ReadInt(), 0);

        [Test, Category("Primitives")]
        public Task Primitives_Int_One()
            => RoundTrip((w, v) => w.Writer.WriteInt(v), r => r.Reader.ReadInt(), 1);

        [Test, Category("Primitives")]
        public Task Primitives_Int_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteInt(v), r => r.Reader.ReadInt(), int.MaxValue);

        // ── UInt ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteUInt(v), r => r.Reader.ReadUInt(), 0u);

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MidValue()
            => RoundTrip((w, v) => w.Writer.WriteUInt(v), r => r.Reader.ReadUInt(), 2147483648u);

        [Test, Category("Primitives")]
        public Task Primitives_UInt_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteUInt(v), r => r.Reader.ReadUInt(), uint.MaxValue);

        // ── Long ─────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Long_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteLong(v), r => r.Reader.ReadLong(), long.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Long_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteLong(v), r => r.Reader.ReadLong(), -1L);

        [Test, Category("Primitives")]
        public Task Primitives_Long_Zero()
            => RoundTrip((w, v) => w.Writer.WriteLong(v), r => r.Reader.ReadLong(), 0L);

        [Test, Category("Primitives")]
        public Task Primitives_Long_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteLong(v), r => r.Reader.ReadLong(), long.MaxValue);

        // ── ULong ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteULong(v), r => r.Reader.ReadULong(), 0uL);

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MidValue()
            => RoundTrip((w, v) => w.Writer.WriteULong(v), r => r.Reader.ReadULong(), (ulong)long.MaxValue + 1uL);

        [Test, Category("Primitives")]
        public Task Primitives_ULong_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteULong(v), r => r.Reader.ReadULong(), ulong.MaxValue);

        // ── Float ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Float_Zero()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), 0f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_One()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), 1f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), -1f);

        [Test, Category("Primitives")]
        public Task Primitives_Float_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), float.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Float_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), float.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Float_Epsilon()
            => RoundTrip((w, v) => w.Writer.WriteFloat(v), r => r.Reader.ReadFloat(), float.Epsilon);

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NegativeZero()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteFloat(-0f);
            await CompleteFetch(writer, reader);
            float result = reader.Reader.ReadFloat();
            // Verify the bit pattern is preserved (-0f ≠ +0f at the bit level)
            Assert.That(BitConverter.SingleToUInt32Bits(result),
                Is.EqualTo(BitConverter.SingleToUInt32Bits(-0f)),
                "Negative zero must be preserved bitwise.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NaN()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteFloat(float.NaN);
            await CompleteFetch(writer, reader);
            Assert.That(float.IsNaN(reader.Reader.ReadFloat()), Is.True, "NaN must round-trip as NaN.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_PositiveInfinity()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteFloat(float.PositiveInfinity);
            await CompleteFetch(writer, reader);
            Assert.That(reader.Reader.ReadFloat(), Is.EqualTo(float.PositiveInfinity));
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Float_NegativeInfinity()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteFloat(float.NegativeInfinity);
            await CompleteFetch(writer, reader);
            Assert.That(reader.Reader.ReadFloat(), Is.EqualTo(float.NegativeInfinity));
        }

        // ── Double ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Double_Zero()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), 0d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_One()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), 1d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), -1d);

        [Test, Category("Primitives")]
        public Task Primitives_Double_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), double.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Double_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), double.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Double_Epsilon()
            => RoundTrip((w, v) => w.Writer.WriteDouble(v), r => r.Reader.ReadDouble(), double.Epsilon);

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NegativeZero()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteDouble(-0d);
            await CompleteFetch(writer, reader);
            double result = reader.Reader.ReadDouble();
            Assert.That(BitConverter.DoubleToUInt64Bits(result),
                Is.EqualTo(BitConverter.DoubleToUInt64Bits(-0d)),
                "Negative zero must be preserved bitwise.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NaN()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteDouble(double.NaN);
            await CompleteFetch(writer, reader);
            Assert.That(double.IsNaN(reader.Reader.ReadDouble()), Is.True, "NaN must round-trip as NaN.");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_PositiveInfinity()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteDouble(double.PositiveInfinity);
            await CompleteFetch(writer, reader);
            Assert.That(reader.Reader.ReadDouble(), Is.EqualTo(double.PositiveInfinity));
        }

        [Test, Category("Primitives")]
        public async Task Primitives_Double_NegativeInfinity()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteDouble(double.NegativeInfinity);
            await CompleteFetch(writer, reader);
            Assert.That(reader.Reader.ReadDouble(), Is.EqualTo(double.NegativeInfinity));
        }

        // ── Decimal ──────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_Zero()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), 0m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_One()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), 1m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_NegativeOne()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), -1m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_MinValue()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), decimal.MinValue);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_MaxValue()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), decimal.MaxValue);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_HighPrecision()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), 0.0000000000000000000000000001m);

        [Test, Category("Primitives")]
        public Task Primitives_Decimal_Pi()
            => RoundTrip((w, v) => w.Writer.WriteDecimal(v), r => r.Reader.ReadDecimal(), 3.14159265358979323846m);

        // ── String ───────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_String_Null()
            => RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), (string?)null);

        [Test, Category("Primitives")]
        public Task Primitives_String_Empty()
            => RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), "");

        [Test, Category("Primitives")]
        public Task Primitives_String_Ascii()
            => RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), "Hello, world!");

        [Test, Category("Primitives")]
        public Task Primitives_String_Unicode()
            => RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), "Привет мир 🌍 中文 日本語");

        [Test, Category("Primitives")]
        public Task Primitives_String_EmbeddedNullChar()
            => RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), "a\0b");

        [Test, Category("Primitives")]
        public async Task Primitives_String_Long()
        {
            string longStr = new string('x', 10_000);
            await RoundTrip((w, v) => w.Writer.WriteString(v), r => r.Reader.ReadString(), longStr);
        }

        // ── Array ────────────────────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public Task Primitives_Array_Null()
            => RoundTrip((w, v) => w.Writer.WriteArray(v), r => r.Reader.ReadArray(), (byte[]?)null);

        [Test, Category("Primitives")]
        public Task Primitives_Array_Empty()
            => RoundTrip((w, v) => w.Writer.WriteArray(v), r => r.Reader.ReadArray(), Array.Empty<byte>());

        [Test, Category("Primitives")]
        public Task Primitives_Array_SingleByte()
            => RoundTrip((w, v) => w.Writer.WriteArray(v), r => r.Reader.ReadArray(), new byte[] { 42 });

        [Test, Category("Primitives")]
        public Task Primitives_Array_MultipleBytes()
            => RoundTrip((w, v) => w.Writer.WriteArray(v), r => r.Reader.ReadArray(), new byte[] { 0, 1, 127, 128, 255 });

        [Test, Category("Primitives")]
        public async Task Primitives_Array_Large()
        {
            byte[] buf = new byte[64 * 1024];
            for (int i = 0; i < buf.Length; i++) buf[i] = (byte)(i % 256);
            await RoundTrip((w, v) => w.Writer.WriteArray(v), r => r.Reader.ReadArray(), buf);
        }

        // ── Multi-value sequences ─────────────────────────────────────────────────

        [Test, Category("Primitives")]
        public async Task Primitives_AllTypes_Sequential_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();

            writer.Writer.WriteBool(true);
            writer.Writer.WriteByte(0xFF);
            writer.Writer.WriteSByte(-42);
            writer.Writer.WriteChar('Z');
            writer.Writer.WriteShort(-1000);
            writer.Writer.WriteUShort(60000);
            writer.Writer.WriteInt(-100_000);
            writer.Writer.WriteUInt(4_000_000_000u);
            writer.Writer.WriteLong(long.MinValue);
            writer.Writer.WriteULong(ulong.MaxValue);
            writer.Writer.WriteFloat(3.14f);
            writer.Writer.WriteDouble(2.718281828d);
            writer.Writer.WriteDecimal(1.23456789012345678901234567m);
            writer.Writer.WriteString("hello");
            writer.Writer.WriteString(null);
            writer.Writer.WriteArray(new byte[] { 1, 2, 3 });
            writer.Writer.WriteArray(null);

            await CompleteFetch(writer, reader);

            Assert.That(reader.Reader.ReadBool(),    Is.EqualTo(true));
            Assert.That(reader.Reader.ReadByte(),    Is.EqualTo((byte)0xFF));
            Assert.That(reader.Reader.ReadSByte(),   Is.EqualTo((sbyte)-42));
            Assert.That(reader.Reader.ReadChar(),    Is.EqualTo('Z'));
            Assert.That(reader.Reader.ReadShort(),   Is.EqualTo((short)-1000));
            Assert.That(reader.Reader.ReadUShort(),  Is.EqualTo((ushort)60000));
            Assert.That(reader.Reader.ReadInt(),     Is.EqualTo(-100_000));
            Assert.That(reader.Reader.ReadUInt(),    Is.EqualTo(4_000_000_000u));
            Assert.That(reader.Reader.ReadLong(),    Is.EqualTo(long.MinValue));
            Assert.That(reader.Reader.ReadULong(),   Is.EqualTo(ulong.MaxValue));
            Assert.That(reader.Reader.ReadFloat(),   Is.EqualTo(3.14f));
            Assert.That(reader.Reader.ReadDouble(),  Is.EqualTo(2.718281828d));
            Assert.That(reader.Reader.ReadDecimal(), Is.EqualTo(1.23456789012345678901234567m));
            Assert.That(reader.Reader.ReadString(),  Is.EqualTo("hello"));
            Assert.That(reader.Reader.ReadString(),  Is.Null);
            Assert.That(reader.Reader.ReadArray(),   Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(reader.Reader.ReadArray(),   Is.Null);
        }

        [Test, Category("Primitives")]
        public async Task Primitives_SameType_MultipleValues_Sequential()
        {
            // Write 100 ints in sequence to stress sequential consistency.
            var (writer, reader) = await GetWriterReaderPair();
            const int count = 100;
            for (int i = 0; i < count; i++)
                writer.Writer.WriteInt(i);
            await CompleteFetch(writer, reader);
            for (int i = 0; i < count; i++)
                Assert.That(reader.Reader.ReadInt(), Is.EqualTo(i), $"Mismatch at index {i}");
        }

        [Test, Category("Primitives")]
        public async Task Primitives_StringAndArrayInterleaved()
        {
            // Verify that null/non-null strings and arrays are correctly distinguished.
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteString("first");
            writer.Writer.WriteArray(new byte[] { 10, 20 });
            writer.Writer.WriteString(null);
            writer.Writer.WriteArray(Array.Empty<byte>());
            writer.Writer.WriteArray(null);
            writer.Writer.WriteString("");

            await CompleteFetch(writer, reader);

            Assert.That(reader.Reader.ReadString(), Is.EqualTo("first"));
            Assert.That(reader.Reader.ReadArray(),  Is.EqualTo(new byte[] { 10, 20 }));
            Assert.That(reader.Reader.ReadString(), Is.Null);
            Assert.That(reader.Reader.ReadArray(),  Is.EqualTo(Array.Empty<byte>()));
            Assert.That(reader.Reader.ReadArray(),  Is.Null);
            Assert.That(reader.Reader.ReadString(), Is.EqualTo(""));
        }
    }
}

