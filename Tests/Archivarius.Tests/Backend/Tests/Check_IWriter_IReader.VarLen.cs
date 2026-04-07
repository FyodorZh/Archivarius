using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    /// <summary>
    /// Tests targeting variable-length (VarInt/LEB128-style) encoding boundaries.
    /// Fixed-width implementations pass trivially. Variable-length implementations
    /// must correctly handle values at byte-count threshold boundaries.
    /// </summary>
    public abstract partial class Check_IWriter_IReader
    {
        // ── VarLen helper ────────────────────────────────────────────────────────

        private async Task RoundTripAll<T>(T[] values, Action<IWriter, T> write, Func<IReader, T> read)
        {
            var (writer, reader) = await GetWriterReaderPair();
            foreach (var v in values)
                write(writer, v);
            await CompleteFetch(writer, reader);
            foreach (var v in values)
                Assert.That(read(reader), Is.EqualTo(v), $"Round-trip mismatch for value {v}");
        }

        // ── Int ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Covers signed int values at every 7-bit LEB128 / ZigZag boundary, including
        /// 0, ±1, ±63/64, ±8191/8192, ±1048575/1048576, ±134217727/134217728, min/max.
        /// </summary>
        [Test, Category("VarLen")]
        public Task VarLen_Int_KeyBoundaryValues()
            => RoundTripAll(
                new[]
                {
                    0, 1, -1,
                    63, 64, -64, -65,
                    127, 128, -128, -129,
                    8191, 8192, -8192, -8193,
                    16383, 16384, -16384, -16385,
                    1_048_575, 1_048_576, -1_048_576, -1_048_577,
                    134_217_727, 134_217_728, -134_217_728, -134_217_729,
                    int.MaxValue - 1, int.MaxValue,
                    int.MinValue + 1, int.MinValue
                },
                (w, v) => w.WriteInt(v),
                r => r.ReadInt());

        // ── UInt ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Covers unsigned int values at every 7-bit LEB128 boundary (0, 127, 128, 16383,
        /// 16384, 2097151, 2097152, 268435455, 268435456, uint.MaxValue).
        /// </summary>
        [Test, Category("VarLen")]
        public Task VarLen_UInt_KeyBoundaryValues()
            => RoundTripAll(
                new uint[]
                {
                    0u, 1u,
                    127u, 128u,
                    16_383u, 16_384u,
                    2_097_151u, 2_097_152u,
                    268_435_455u, 268_435_456u,
                    uint.MaxValue - 1u, uint.MaxValue
                },
                (w, v) => w.WriteUInt(v),
                r => r.ReadUInt());

        // ── Long ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Covers signed long values at 7-bit LEB128 / ZigZag boundaries, plus extremes.
        /// </summary>
        [Test, Category("VarLen")]
        public Task VarLen_Long_KeyBoundaryValues()
            => RoundTripAll(
                new long[]
                {
                    0L, 1L, -1L,
                    63L, 64L, -64L, -65L,
                    8_191L, 8_192L, -8_192L, -8_193L,
                    1_048_575L, 1_048_576L, -1_048_576L, -1_048_577L,
                    72_057_594_037_927_935L, 72_057_594_037_927_936L,
                    -72_057_594_037_927_936L, -72_057_594_037_927_937L,
                    long.MaxValue - 1L, long.MaxValue,
                    long.MinValue + 1L, long.MinValue
                },
                (w, v) => w.WriteLong(v),
                r => r.ReadLong());

        // ── ULong ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Covers unsigned long values at 7-bit LEB128 boundaries, plus extremes.
        /// </summary>
        [Test, Category("VarLen")]
        public Task VarLen_ULong_KeyBoundaryValues()
            => RoundTripAll(
                new ulong[]
                {
                    0uL, 1uL,
                    127uL, 128uL,
                    16_383uL, 16_384uL,
                    2_097_151uL, 2_097_152uL,
                    (1uL << 56) - 1uL, 1uL << 56,
                    ulong.MaxValue - 1uL, ulong.MaxValue
                },
                (w, v) => w.WriteULong(v),
                r => r.ReadULong());

        // ── Short ────────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_Short_KeyBoundaryValues()
            => RoundTripAll(
                new short[]
                {
                    0, 1, -1,
                    63, 64, -64, -65,
                    127, 128, -128, -129,
                    8_191, 8_192, -8_192, -8_193,
                    short.MaxValue - 1, short.MaxValue,
                    short.MinValue + 1, short.MinValue
                },
                (w, v) => w.WriteShort(v),
                r => r.ReadShort());

        // ── UShort ───────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_UShort_KeyBoundaryValues()
            => RoundTripAll(
                new ushort[]
                {
                    0, 1,
                    127, 128,
                    16_383, 16_384,
                    ushort.MaxValue - 1, ushort.MaxValue
                },
                (w, v) => w.WriteUShort(v),
                r => r.ReadUShort());

        // ── Byte ─────────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_Byte_KeyBoundaryValues()
            => RoundTripAll(
                new byte[] { 0, 1, 63, 64, 127, 128, 254, 255 },
                (w, v) => w.WriteByte(v),
                r => r.ReadByte());

        // ── SByte ────────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_SByte_KeyBoundaryValues()
            => RoundTripAll(
                new sbyte[] { sbyte.MinValue, -65, -64, -1, 0, 63, 64, sbyte.MaxValue },
                (w, v) => w.WriteSByte(v),
                r => r.ReadSByte());

        // ── Char ─────────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_Char_KeyBoundaryValues()
            => RoundTripAll(
                new char[] { '\0', '\x7F', '\x80', '\xFF', '\u0100', '\u7FFF', '\u8000', char.MaxValue },
                (w, v) => w.WriteChar(v),
                r => r.ReadChar());

        // ── Float bit patterns ───────────────────────────────────────────────────

        /// <summary>
        /// Verifies that unusual float bit patterns (subnormals, near-boundary, NaN, ±∞, −0) are preserved.
        /// </summary>
        [Test, Category("VarLen")]
        public async Task VarLen_Float_SpecialBitPatterns()
        {
            float[] values =
            {
                0f, -0f,
                float.Epsilon,             // smallest positive subnormal
                1.1754944e-38f,            // smallest normal
                float.MaxValue,
                float.MinValue,
                float.PositiveInfinity,
                float.NegativeInfinity,
                1f, -1f,
                0.5f, -0.5f,
                (float)Math.PI
            };

            var (writer, reader) = await GetWriterReaderPair();
            foreach (var v in values)
                writer.WriteFloat(v);
            await CompleteFetch(writer, reader);

            foreach (var v in values)
            {
                float result = reader.ReadFloat();
                if (float.IsNaN(v))
                {
                    Assert.That(float.IsNaN(result), Is.True, "NaN must round-trip as NaN.");
                }
                else
                {
                    Assert.That(BitConverter.SingleToUInt32Bits(result),
                        Is.EqualTo(BitConverter.SingleToUInt32Bits(v)),
                        $"Bit-exact round-trip failed for float {v}");
                }
            }
        }

        // ── Double bit patterns ──────────────────────────────────────────────────

        /// <summary>
        /// Verifies that unusual double bit patterns are preserved.
        /// </summary>
        [Test, Category("VarLen")]
        public async Task VarLen_Double_SpecialBitPatterns()
        {
            double[] values =
            {
                0d, -0d,
                double.Epsilon,
                2.2250738585072014e-308,   // smallest normal
                double.MaxValue,
                double.MinValue,
                double.PositiveInfinity,
                double.NegativeInfinity,
                1d, -1d,
                0.5, -0.5,
                Math.PI, Math.E
            };

            var (writer, reader) = await GetWriterReaderPair();
            foreach (var v in values)
                writer.WriteDouble(v);
            await CompleteFetch(writer, reader);

            foreach (var v in values)
            {
                double result = reader.ReadDouble();
                if (double.IsNaN(v))
                {
                    Assert.That(double.IsNaN(result), Is.True, "NaN must round-trip as NaN.");
                }
                else
                {
                    Assert.That(BitConverter.DoubleToUInt64Bits(result),
                        Is.EqualTo(BitConverter.DoubleToUInt64Bits(v)),
                        $"Bit-exact round-trip failed for double {v}");
                }
            }
        }

        // ── Decimal ──────────────────────────────────────────────────────────────

        [Test, Category("VarLen")]
        public Task VarLen_Decimal_KeyValues()
            => RoundTripAll(
                new[]
                {
                    0m, 1m, -1m,
                    decimal.MinValue, decimal.MaxValue,
                    0.0000000000000000000000000001m,  // max precision
                    79228162514264337593543950335m,   // max value without fractional
                    1.0m, 1.00m, 1.000m,              // same value, different scale (may differ)
                },
                (w, v) => w.WriteDecimal(v),
                r => r.ReadDecimal());

        // ── String length boundaries ─────────────────────────────────────────────

        [Test, Category("VarLen")]
        public async Task VarLen_String_LengthBoundaries()
        {
            // Lengths 1, 127 (1-byte LEB128 boundary), 128, 16383, 16384
            int[] lengths = { 1, 127, 128, 16_383, 16_384 };
            foreach (int len in lengths)
            {
                var (writer, reader) = await GetWriterReaderPair();
                string s = new string('A', len);
                writer.WriteString(s);
                await CompleteFetch(writer, reader);
                Assert.That(reader.ReadString(), Is.EqualTo(s),
                    $"String of length {len} failed round-trip.");
            }
        }

        [Test, Category("VarLen")]
        public async Task VarLen_String_MultibyteLengthCrossesLengthBoundary()
        {
            // A string whose UTF-8 byte length is ≥ 128 even though the char count is smaller
            // (3-byte UTF-8 chars): 43 × '中' = 43 × 3 bytes = 129 bytes
            var (writer, reader) = await GetWriterReaderPair();
            string s = new string('\u4E2D', 43);
            writer.WriteString(s);
            await CompleteFetch(writer, reader);
            Assert.That(reader.ReadString(), Is.EqualTo(s));
        }

        // ── Array/Bytes length boundaries ────────────────────────────────────────

        [Test, Category("VarLen")]
        public async Task VarLen_Array_LengthBoundaries()
        {
            int[] lengths = { 1, 127, 128, 16_383, 16_384 };
            foreach (int len in lengths)
            {
                var (writer, reader) = await GetWriterReaderPair();
                byte[] arr = new byte[len];
                for (int i = 0; i < len; i++) arr[i] = (byte)(i % 256);
                writer.WriteArray(arr);
                await CompleteFetch(writer, reader);
                Assert.That(reader.ReadArray(), Is.EqualTo(arr),
                    $"Array of length {len} failed round-trip.");
            }
        }

        [Test, Category("VarLen")]
        public async Task VarLen_WriteBytes_CountBoundaries()
        {
            int[] counts = { 1, 127, 128, 16_383, 16_384 };
            foreach (int count in counts)
            {
                var (writer, reader) = await GetWriterReaderPair();
                byte[] src = new byte[count];
                for (int i = 0; i < count; i++) src[i] = (byte)(i % 256);
                byte[] dst = new byte[count];
                writer.WriteBytes(src, 0, count);
                await CompleteFetch(writer, reader);
                reader.ReadBytes(dst, 0, count);
                Assert.That(dst, Is.EqualTo(src), $"WriteBytes/ReadBytes with count={count} failed.");
            }
        }
    }
}

