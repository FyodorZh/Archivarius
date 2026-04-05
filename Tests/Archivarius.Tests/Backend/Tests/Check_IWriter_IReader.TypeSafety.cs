using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ════════════════════════════════════════════════════════════════════════
        // TYPE-SAFE MODE (requires typeSafety == true)
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Helper: writes <paramref name="writeValue"/>, flushes/fetches, then asserts that
        /// calling <paramref name="wrongRead"/> throws <see cref="InvalidDataException"/> immediately.
        /// </summary>
        private async Task AssertTypeMismatch<W>(
            Action<IWriter, W> write,
            W writeValue,
            Action<IReader> wrongRead,
            string description)
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore($"Type safety not supported by this implementation (test: {description}).");

            write(writer, writeValue);
            await FlushFetch(writer, reader);

            Assert.Throws<InvalidDataException>(
                () => wrongRead(reader),
                $"Expected InvalidDataException for type mismatch: {description}");
        }

        // ── Bool mismatches ───────────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadByte_Throws()
            => AssertTypeMismatch((w, v) => w.WriteBool(v), true, r => r.ReadByte(), "WriteBool → ReadByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadSByte_Throws()
            => AssertTypeMismatch((w, v) => w.WriteBool(v), true, r => r.ReadSByte(), "WriteBool → ReadSByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteBool(v), true, r => r.ReadInt(), "WriteBool → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.WriteBool(v), true, r => r.ReadString(), "WriteBool → ReadString");

        // ── Byte / SByte mismatches ───────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteByte_ReadSByte_Throws()
            => AssertTypeMismatch((w, v) => w.WriteByte(v), (byte)1, r => r.ReadSByte(), "WriteByte → ReadSByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteByte_ReadBool_Throws()
            => AssertTypeMismatch((w, v) => w.WriteByte(v), (byte)1, r => r.ReadBool(), "WriteByte → ReadBool");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteSByte_ReadByte_Throws()
            => AssertTypeMismatch((w, v) => w.WriteSByte(v), (sbyte)1, r => r.ReadByte(), "WriteSByte → ReadByte");

        // ── Char mismatches ───────────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteChar_ReadUShort_Throws()
            => AssertTypeMismatch((w, v) => w.WriteChar(v), 'A', r => r.ReadUShort(), "WriteChar → ReadUShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteChar_ReadShort_Throws()
            => AssertTypeMismatch((w, v) => w.WriteChar(v), 'A', r => r.ReadShort(), "WriteChar → ReadShort");

        // ── Short / UShort mismatches ─────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteShort_ReadUShort_Throws()
            => AssertTypeMismatch((w, v) => w.WriteShort(v), (short)1, r => r.ReadUShort(), "WriteShort → ReadUShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteShort_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteShort(v), (short)1, r => r.ReadInt(), "WriteShort → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUShort_ReadShort_Throws()
            => AssertTypeMismatch((w, v) => w.WriteUShort(v), (ushort)1, r => r.ReadShort(), "WriteUShort → ReadShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUShort_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteUShort(v), (ushort)1, r => r.ReadInt(), "WriteUShort → ReadInt");

        // ── Int / UInt mismatches ─────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadUInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteInt(v), 42, r => r.ReadUInt(), "WriteInt → ReadUInt");

        /// <summary>Same bit width, different type — must still be detected.</summary>
        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadFloat_Throws()
            => AssertTypeMismatch((w, v) => w.WriteInt(v), 42, r => r.ReadFloat(), "WriteInt → ReadFloat");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.WriteInt(v), 42, r => r.ReadLong(), "WriteInt → ReadLong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUInt_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteUInt(v), 42u, r => r.ReadInt(), "WriteUInt → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteFloat_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteFloat(v), 1f, r => r.ReadInt(), "WriteFloat → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteFloat_ReadDouble_Throws()
            => AssertTypeMismatch((w, v) => w.WriteFloat(v), 1f, r => r.ReadDouble(), "WriteFloat → ReadDouble");

        // ── Long / ULong mismatches ───────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteLong_ReadULong_Throws()
            => AssertTypeMismatch((w, v) => w.WriteLong(v), 1L, r => r.ReadULong(), "WriteLong → ReadULong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteLong_ReadDouble_Throws()
            => AssertTypeMismatch((w, v) => w.WriteLong(v), 1L, r => r.ReadDouble(), "WriteLong → ReadDouble");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteULong_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.WriteULong(v), 1uL, r => r.ReadLong(), "WriteULong → ReadLong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDouble_ReadFloat_Throws()
            => AssertTypeMismatch((w, v) => w.WriteDouble(v), 1d, r => r.ReadFloat(), "WriteDouble → ReadFloat");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDouble_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.WriteDouble(v), 1d, r => r.ReadLong(), "WriteDouble → ReadLong");

        // ── Decimal / String / Array mismatches ───────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDecimal_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.WriteDecimal(v), 1m, r => r.ReadString(), "WriteDecimal → ReadString");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteString_ReadArray_Throws()
            => AssertTypeMismatch((w, v) => w.WriteString(v), "hello", r => r.ReadArray(), "WriteString → ReadArray");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteArray_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.WriteArray(v), new byte[] { 1, 2 }, r => r.ReadString(), "WriteArray → ReadString");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteString_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.WriteString(v), "hi", r => r.ReadInt(), "WriteString → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.WriteInt(v), 99, r => r.ReadString(), "WriteInt → ReadString");

        // ── Null variants are typed ───────────────────────────────────────────────

        /// <summary>WriteString(null) and WriteArray(null) are distinct typed values when typeSafety=true.</summary>
        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteNullString_ReadArray_Throws()
            => AssertTypeMismatch((w, v) => w.WriteString(v), (string?)null, r => r.ReadArray(), "WriteString(null) → ReadArray");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteNullArray_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.WriteArray(v), (byte[]?)null, r => r.ReadString(), "WriteArray(null) → ReadString");

        // ── WriteBytes / ReadBytes type mismatch ──────────────────────────────────

        /// <summary>WriteBytes then a typed Read*() must throw InvalidDataException immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadInt_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteBytes(new byte[] { 0, 0, 0, 0 }, 0, 4);
            await FlushFetch(writer, reader);

            Assert.Throws<InvalidDataException>(() => reader.ReadInt(),
                "WriteBytes followed by ReadInt() must throw immediately (type mismatch).");
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadFloat_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteBytes(new byte[] { 0, 0, 0, 0 }, 0, 4);
            await FlushFetch(writer, reader);

            Assert.Throws<InvalidDataException>(() => reader.ReadFloat());
        }

        /// <summary>A typed write (WriteInt) followed by ReadBytes must throw immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteInt_ReadBytes_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteInt(42);
            await FlushFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.ReadBytes(dst, 0, 4),
                "WriteInt followed by ReadBytes must throw immediately (type mismatch).");
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteFloat_ReadBytes_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteFloat(1.5f);
            await FlushFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.ReadBytes(dst, 0, 4));
        }

        // ── WriteBytes count mismatch ─────────────────────────────────────────────

        /// <summary>
        /// WriteBytes(5) paired with ReadBytes(3): mismatch must be detected immediately at the
        /// ReadBytes call (not deferred to section end or stream end).
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes5_ReadBytes3_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteBytes(new byte[5], 0, 5);
            await FlushFetch(writer, reader);

            byte[] dst = new byte[5];
            bool threw = false;
            try { reader.ReadBytes(dst, 0, 3); }
            catch (InvalidDataException) { threw = true; }

            Assert.That(threw, Is.True,
                "Count mismatch (wrote 5, reading 3) must throw InvalidDataException immediately at ReadBytes.");
        }

        /// <summary>WriteBytes(3) paired with ReadBytes(7): mismatch detected immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes3_ReadBytes7_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteBytes(new byte[3], 0, 3);
            await FlushFetch(writer, reader);

            byte[] dst = new byte[10];
            Assert.Throws<InvalidDataException>(() => reader.ReadBytes(dst, 0, 7));
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes0_ReadBytes1_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type un-safety not supported.");

            writer.WriteBytes(new byte[1], 0, 0);  // explicitly zero bytes
            await FlushFetch(writer, reader);

            byte[] dst = new byte[1];
            Assert.Throws<EndOfStreamException>(() => reader.ReadBytes(dst, 0, 1));
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes1_ReadBytes2_ThrowsImmediately()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            writer.WriteBytes(new byte[] { 42 }, 0, 1);
            await FlushFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.ReadBytes(dst, 0, 2));
        }

        /// <summary>Matching WriteBytes/ReadBytes counts must NOT throw (sanity check).</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadBytes_SameCount_NoThrow()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");

            byte[] src = { 1, 2, 3, 4, 5 };
            byte[] dst = new byte[5];
            writer.WriteBytes(src, 0, 5);
            await FlushFetch(writer, reader);
            Assert.DoesNotThrow(() => reader.ReadBytes(dst, 0, 5));
            Assert.That(dst, Is.EqualTo(src));
        }

        /// <summary>
        /// Confirms the mismatch fires on Read*() itself, not deferred to EndSection()
        /// even when sections are enabled.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_MismatchIsImmediate_NotDeferredToEndSection()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (!typeSafety)
                Assert.Ignore("Type safety not supported.");
            if (writer.SectionsSupport == CapabilityState.NotSupported)
                Assert.Ignore("Sections not supported (needed for this test to have context).");

            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(42);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();

            bool exceptionOnRead = false;
            try { reader.ReadFloat(); }  // mismatch: wrote Int, reading Float
            catch (InvalidDataException) { exceptionOnRead = true; }

            Assert.That(exceptionOnRead, Is.True,
                "InvalidDataException must be thrown by ReadFloat(), not deferred to EndSection().");
        }

        // ════════════════════════════════════════════════════════════════════════
        // WEAK SAFETY (typeSafety == false)
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// When typeSafety=false and sections are supported, the reader's EndSection()
        /// must report (return false) when bytes inside the section were not fully consumed.
        /// This is the minimum weak-safety guarantee.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_Sections_UnderConsumption_EndSectionReturnsFalse()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (typeSafety)
                Assert.Ignore("This test is for weak-safety (typeSafety=false) only.");
            if (writer.SectionsSupport == CapabilityState.NotSupported)
                Assert.Ignore("Sections required for this weak-safety test.");

            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            writer.WriteInt(2);  // will not be consumed
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();   // consume only one
            bool skipped = reader.EndSection();
            Assert.That(skipped, Is.False,
                "EndSection must return false when unconsumed bytes remain (weak safety).");
        }

        /// <summary>
        /// Reading beyond the section boundary must throw EndOfStreamException even in weak-safety mode.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_Sections_OverConsumption_ThrowsEndOfStream()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (typeSafety)
                Assert.Ignore("This test is for weak-safety (typeSafety=false) only.");
            if (writer.SectionsSupport == CapabilityState.NotSupported)
                Assert.Ignore("Sections required for this weak-safety test.");

            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(42);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();
            Assert.Throws<EndOfStreamException>(() => reader.ReadInt(),
                "Reading beyond section boundary must throw EndOfStreamException (weak safety).");
        }

        /// <summary>Exact consumption → EndSection returns true (correct alignment).</summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_Sections_ExactConsumption_EndSectionReturnsTrue()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (typeSafety)
                Assert.Ignore("This test is for weak-safety (typeSafety=false) only.");
            if (writer.SectionsSupport == CapabilityState.NotSupported)
                Assert.Ignore("Sections required.");

            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(7);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True);
        }

        /// <summary>
        /// When typeSafety=false and sections are NOT supported, reading past the end of the
        /// available data must throw (EndOfStreamException or similar).
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_NoSections_ReadBeyondEnd_Throws()
        {
            var (writer, reader, typeSafety) = await GetWriterReaderPair();
            if (typeSafety)
                Assert.Ignore("This test is for weak-safety (typeSafety=false) only.");
            if (writer.SectionsSupport != CapabilityState.NotSupported)
                Assert.Ignore("This test targets section-less weak-safety (SectionsSupport must be NotSupported).");

            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            reader.ReadInt();  // consume the only int

            Assert.Throws<EndOfStreamException>(() => reader.ReadInt(),
                "Reading past the end must throw EndOfStreamException in weak-safety mode.");
        }
    }
}

