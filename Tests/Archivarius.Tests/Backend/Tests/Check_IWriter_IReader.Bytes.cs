using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ── WriteBytes / ReadBytes round-trips ───────────────────────────────────

        [Test, Category("Bytes")]
        public async Task Bytes_SingleByte_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = { 0xAB };
            byte[] dst = new byte[1];
            writer.WriteBytes(src, 0, 1);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 1);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_MultipleBytes_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = { 1, 2, 3, 4, 5 };
            byte[] dst = new byte[5];
            writer.WriteBytes(src, 0, 5);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 5);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_WithNonZeroWriteOffset_CorrectSliceTransmitted()
        {
            // Write bytes [2..4] of an 8-byte buffer (offset=2, count=3).
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = { 0, 10, 20, 30, 40, 50, 60, 70 };
            byte[] dst = new byte[3];
            writer.WriteBytes(src, 2, 3);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 3);
            Assert.That(dst, Is.EqualTo(new byte[] { 20, 30, 40 }));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_WithNonZeroReadOffset_BytesPlacedAtCorrectPosition()
        {
            // Read into dst[2..4]; dst[0] and dst[5] must remain untouched.
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = { 10, 20, 30 };
            byte[] dst = new byte[6];    // [0]=0, [1]=0, read into [2..4], [5]=0
            writer.WriteBytes(src, 0, 3);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 2, 3);
            Assert.That(dst[0], Is.EqualTo(0), "Byte before offset must not be touched.");
            Assert.That(dst[1], Is.EqualTo(0), "Byte before offset must not be touched.");
            Assert.That(dst[2], Is.EqualTo(10));
            Assert.That(dst[3], Is.EqualTo(20));
            Assert.That(dst[4], Is.EqualTo(30));
            Assert.That(dst[5], Is.EqualTo(0), "Byte after range must not be touched.");
        }

        [Test, Category("Bytes")]
        public async Task Bytes_ZeroCount_Write_IsNoop()
        {
            // WriteBytes with count=0 adds nothing to the stream; the subsequent ReadInt works.
            var (writer, reader) = await GetWriterReaderPair();
            byte[] dummy = { 99 };
            writer.WriteBytes(dummy, 0, 0);   // no-op
            writer.WriteInt(42);
            await CompleteFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(42));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_ZeroCount_Read_IsNoop()
        {
            // ReadBytes with count=0 consumes nothing; the subsequent ReadInt works.
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(77);
            await CompleteFetch(writer, reader);
            byte[] dst = new byte[4];
            reader.ReadBytes(dst, 0, 0);      // no-op
            Assert.That(reader.ReadInt(), Is.EqualTo(77));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_LargeBuffer_RoundTrip()
        {
            const int size = 100 * 1024;
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = new byte[size];
            for (int i = 0; i < size; i++) src[i] = (byte)(i * 7 % 256);
            byte[] dst = new byte[size];
            writer.WriteBytes(src, 0, size);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, size);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_TwoConsecutivePairs_BothCorrect()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src1 = { 1, 2, 3 };
            byte[] src2 = { 10, 20, 30, 40 };
            writer.WriteBytes(src1, 0, 3);
            writer.WriteBytes(src2, 0, 4);
            await CompleteFetch(writer, reader);
            byte[] dst1 = new byte[3];
            byte[] dst2 = new byte[4];
            reader.ReadBytes(dst1, 0, 3);
            reader.ReadBytes(dst2, 0, 4);
            Assert.That(dst1, Is.EqualTo(src1));
            Assert.That(dst2, Is.EqualTo(src2));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_AllZeroContent_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = new byte[32];  // all zeros
            byte[] dst = new byte[32];
            writer.WriteBytes(src, 0, 32);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 32);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_AllMaxByteContent_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = new byte[32];
            Array.Fill(src, (byte)0xFF);
            byte[] dst = new byte[32];
            writer.WriteBytes(src, 0, 32);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 32);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_AlternatingPattern_RoundTrip()
        {
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = new byte[64];
            for (int i = 0; i < src.Length; i++) src[i] = (i % 2 == 0) ? (byte)0xAA : (byte)0x55;
            byte[] dst = new byte[64];
            writer.WriteBytes(src, 0, 64);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 0, 64);
            Assert.That(dst, Is.EqualTo(src));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_DoesNotOverwriteBeyondOffsetPlusCount()
        {
            // Guard bytes at dst[0] and dst[count+1] must not change.
            var (writer, reader) = await GetWriterReaderPair();
            byte[] src = { 7, 8, 9 };
            byte[] dst = { 0xFF, 0, 0, 0, 0xFF };   // guard bytes at positions 0 and 4
            writer.WriteBytes(src, 0, 3);
            await CompleteFetch(writer, reader);
            reader.ReadBytes(dst, 1, 3);
            Assert.That(dst[0], Is.EqualTo(0xFF), "Guard byte before range must not change.");
            Assert.That(dst[4], Is.EqualTo(0xFF), "Guard byte after range must not change.");
            Assert.That(dst[1], Is.EqualTo(7));
            Assert.That(dst[2], Is.EqualTo(8));
            Assert.That(dst[3], Is.EqualTo(9));
        }

        // ── Argument validation — WriteBytes ──────────────────────────────────────

        [Test, Category("Bytes")]
        public async Task Bytes_Write_NullBuffer_ThrowsArgumentNullException()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<ArgumentNullException>(() => writer.WriteBytes(null!, 0, 0));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Write_NegativeOffset_ThrowsArgumentOutOfRangeException()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBytes(new byte[4], -1, 2));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Write_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteBytes(new byte[4], 0, -1));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Write_OffsetPlusCountExceedsBuffer_ThrowsArgumentException()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<ArgumentException>(() => writer.WriteBytes(new byte[4], 2, 3));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Write_ZeroCount_NullBuffer_StillThrowsArgumentNullException()
        {
            // Null check must happen even if count == 0 (the zero-count no-op does not bypass null guard).
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<ArgumentNullException>(() => writer.WriteBytes(null!, 0, 0));
        }

        // ── Argument validation — ReadBytes ───────────────────────────────────────

        [Test, Category("Bytes")]
        public async Task Bytes_Read_NullBuffer_ThrowsArgumentNullException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1); // ensure data exists
            await CompleteFetch(writer, reader);
            Assert.Throws<ArgumentNullException>(() => reader.ReadBytes(null!, 0, 0));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Read_NegativeOffset_ThrowsArgumentOutOfRangeException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadBytes(new byte[4], -1, 2));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Read_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadBytes(new byte[4], 0, -1));
        }

        [Test, Category("Bytes")]
        public async Task Bytes_Read_OffsetPlusCountExceedsBuffer_ThrowsArgumentException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            Assert.Throws<ArgumentException>(() => reader.ReadBytes(new byte[4], 2, 3));
        }

        // ── End-of-stream for ReadBytes ───────────────────────────────────────────

        /// <summary>
        /// Writing 2 bytes then calling ReadBytes(4) when type safety is enabled must throw
        /// <see cref="InvalidDataException"/> immediately (count mismatch detected from the type tag).
        /// </summary>
        [Test, Category("Bytes")]
        public async Task Bytes_Read_BeyondEnd_TypeSafe_ThrowsInvalidDataException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);
            // Write 2 bytes but try to read 4
            writer.WriteBytes(new byte[] { 1, 2 }, 0, 2);
            await CompleteFetch(writer, reader);
            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.ReadBytes(dst, 0, 4),
                "ReadBytes(4) when only 2 bytes were written must throw InvalidDataException immediately (type safety).");
        }

        /// <summary>
        /// Writing 2 bytes then calling ReadBytes(4) in weak-safety mode must throw
        /// <see cref="EndOfStreamException"/> when the stream runs out of data.
        /// </summary>
        [Test, Category("Bytes")]
        public async Task Bytes_Read_BeyondEnd_WeakSafety_ThrowsEndOfStreamException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            // Write 2 bytes but try to read 4
            writer.WriteBytes(new byte[] { 1, 2 }, 0, 2);
            await CompleteFetch(writer, reader);
            byte[] dst = new byte[4];
            Assert.Throws<EndOfStreamException>(() => reader.ReadBytes(dst, 0, 4),
                "ReadBytes(4) when only 2 bytes were written must throw EndOfStreamException (weak safety).");
        }
    }
}

