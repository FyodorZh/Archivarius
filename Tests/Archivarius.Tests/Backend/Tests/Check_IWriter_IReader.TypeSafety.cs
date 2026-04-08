using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ════════════════════════════════════════════════════════════════════════
        // TYPE-SAFE MODE
        // Applies to: Mandatory (always enabled) and Supported (explicitly enabled
        // via TrySetTypeSafety(true)). Skipped for NotSupported (type safety unavailable).
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Helper: enables type safety on both writer and reader, writes <paramref name="writeValue"/>,
        /// flushes/fetches, then asserts that calling <paramref name="wrongRead"/> throws
        /// <see cref="InvalidDataException"/> immediately.
        /// Skips automatically when <see cref="CapabilityState.NotSupported"/>.
        /// </summary>
        private async Task AssertTypeMismatch<W>(
            Action<IWriterBackend, W> write,
            W writeValue,
            Action<IReaderBackend> wrongRead,
            string description)
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            write(writer, writeValue);
            await CompleteFetch(writer, reader);

            Assert.Throws<InvalidDataException>(
                () => wrongRead(reader),
                $"Expected InvalidDataException for type mismatch: {description}");
        }

        // ── Bool mismatches ───────────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadByte_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteBool(v), true, r => r.Reader.ReadByte(), "WriteBool → ReadByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadSByte_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteBool(v), true, r => r.Reader.ReadSByte(), "WriteBool → ReadSByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteBool(v), true, r => r.Reader.ReadInt(), "WriteBool → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteBool_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteBool(v), true, r => r.Reader.ReadString(), "WriteBool → ReadString");

        // ── Byte / SByte mismatches ───────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteByte_ReadSByte_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteByte(v), (byte)1, r => r.Reader.ReadSByte(), "WriteByte → ReadSByte");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteByte_ReadBool_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteByte(v), (byte)1, r => r.Reader.ReadBool(), "WriteByte → ReadBool");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteSByte_ReadByte_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteSByte(v), (sbyte)1, r => r.Reader.ReadByte(), "WriteSByte → ReadByte");

        // ── Char mismatches ───────────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteChar_ReadUShort_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteChar(v), 'A', r => r.Reader.ReadUShort(), "WriteChar → ReadUShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteChar_ReadShort_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteChar(v), 'A', r => r.Reader.ReadShort(), "WriteChar → ReadShort");

        // ── Short / UShort mismatches ─────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteShort_ReadUShort_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteShort(v), (short)1, r => r.Reader.ReadUShort(), "WriteShort → ReadUShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteShort_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteShort(v), (short)1, r => r.Reader.ReadInt(), "WriteShort → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUShort_ReadShort_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteUShort(v), (ushort)1, r => r.Reader.ReadShort(), "WriteUShort → ReadShort");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUShort_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteUShort(v), (ushort)1, r => r.Reader.ReadInt(), "WriteUShort → ReadInt");

        // ── Int / UInt mismatches ─────────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadUInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteInt(v), 42, r => r.Reader.ReadUInt(), "WriteInt → ReadUInt");

        /// <summary>Same bit width, different type — must still be detected.</summary>
        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadFloat_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteInt(v), 42, r => r.Reader.ReadFloat(), "WriteInt → ReadFloat");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteInt(v), 42, r => r.Reader.ReadLong(), "WriteInt → ReadLong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteUInt_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteUInt(v), 42u, r => r.Reader.ReadInt(), "WriteUInt → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteFloat_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteFloat(v), 1f, r => r.Reader.ReadInt(), "WriteFloat → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteFloat_ReadDouble_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteFloat(v), 1f, r => r.Reader.ReadDouble(), "WriteFloat → ReadDouble");

        // ── Long / ULong mismatches ───────────────────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteLong_ReadULong_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteLong(v), 1L, r => r.Reader.ReadULong(), "WriteLong → ReadULong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteLong_ReadDouble_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteLong(v), 1L, r => r.Reader.ReadDouble(), "WriteLong → ReadDouble");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteULong_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteULong(v), 1uL, r => r.Reader.ReadLong(), "WriteULong → ReadLong");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDouble_ReadFloat_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteDouble(v), 1d, r => r.Reader.ReadFloat(), "WriteDouble → ReadFloat");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDouble_ReadLong_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteDouble(v), 1d, r => r.Reader.ReadLong(), "WriteDouble → ReadLong");

        // ── Decimal / String / Array mismatches ───────────────────────────────────

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteDecimal_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteDecimal(v), 1m, r => r.Reader.ReadString(), "WriteDecimal → ReadString");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteString_ReadArray_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteString(v), "hello", r => r.Reader.ReadArray(), "WriteString → ReadArray");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteArray_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteArray(v), new byte[] { 1, 2 }, r => r.Reader.ReadString(), "WriteArray → ReadString");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteString_ReadInt_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteString(v), "hi", r => r.Reader.ReadInt(), "WriteString → ReadInt");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteInt_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteInt(v), 99, r => r.Reader.ReadString(), "WriteInt → ReadString");

        // ── Null variants are typed ───────────────────────────────────────────────

        /// <summary>WriteString(null) and WriteArray(null) are distinct typed values when type safety is enabled.</summary>
        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteNullString_ReadArray_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteString(v), (string?)null, r => r.Reader.ReadArray(), "WriteString(null) → ReadArray");

        [Test, Category("TypeSafety")]
        public Task TypeSafety_WriteNullArray_ReadString_Throws()
            => AssertTypeMismatch((w, v) => w.Writer.WriteArray(v), (byte[]?)null, r => r.Reader.ReadString(), "WriteArray(null) → ReadString");

        // ── WriteBytes / ReadBytes type mismatch ──────────────────────────────────

        /// <summary>WriteBytes then a typed Read*() must throw InvalidDataException immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadInt_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[] { 0, 0, 0, 0 }, 0, 4);
            await CompleteFetch(writer, reader);

            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadInt(),
                "WriteBytes followed by ReadInt() must throw immediately (type mismatch).");
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadFloat_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[] { 0, 0, 0, 0 }, 0, 4);
            await CompleteFetch(writer, reader);

            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadFloat());
        }

        /// <summary>A typed write (WriteInt) followed by ReadBytes must throw immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteInt_ReadBytes_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteInt(42);
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadBytes(dst, 0, 4),
                "WriteInt followed by ReadBytes must throw immediately (type mismatch).");
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteFloat_ReadBytes_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteFloat(1.5f);
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadBytes(dst, 0, 4));
        }

        // ── WriteBytes count mismatch ─────────────────────────────────────────────

        /// <summary>
        /// WriteBytes(5) paired with ReadBytes(3): mismatch must be detected immediately at the
        /// ReadBytes call (not deferred to section end or stream end).
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes5_ReadBytes3_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[5], 0, 5);
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[5];
            bool threw = false;
            try { reader.Reader.ReadBytes(dst, 0, 3); }
            catch (InvalidDataException) { threw = true; }

            Assert.That(threw, Is.True,
                "Count mismatch (wrote 5, reading 3) must throw InvalidDataException immediately at ReadBytes.");
        }

        /// <summary>WriteBytes(3) paired with ReadBytes(7): mismatch detected immediately.</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes3_ReadBytes7_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[3], 0, 3);
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[10];
            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadBytes(dst, 0, 7));
        }

        /// <summary>
        /// WriteBytes(buf, 0, 0) is a no-op — nothing is written, not even a type tag.
        /// ReadBytes(dst, 0, 1) then has nothing to read → EndOfStreamException (not InvalidDataException).
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes0_ReadBytes1_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[1], 0, 0);  // explicitly zero bytes — no-op, no tag written
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[1];
            Assert.Throws<EndOfStreamException>(() => reader.Reader.ReadBytes(dst, 0, 1));
        }

        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes1_ReadBytes2_ThrowsImmediately()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            writer.Writer.WriteBytes(new byte[] { 42 }, 0, 1);
            await CompleteFetch(writer, reader);

            byte[] dst = new byte[4];
            Assert.Throws<InvalidDataException>(() => reader.Reader.ReadBytes(dst, 0, 2));
        }

        /// <summary>Matching WriteBytes/ReadBytes counts must NOT throw (sanity check).</summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_WriteBytes_ReadBytes_SameCount_NoThrow()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);

            byte[] src = { 1, 2, 3, 4, 5 };
            byte[] dst = new byte[5];
            writer.Writer.WriteBytes(src, 0, 5);
            await CompleteFetch(writer, reader);
            Assert.DoesNotThrow(() => reader.Reader.ReadBytes(dst, 0, 5));
            Assert.That(dst, Is.EqualTo(src));
        }

        /// <summary>
        /// Confirms the mismatch fires on Read*() itself, not deferred to EndSection()
        /// even when sections are enabled.
        /// Runs for TypeSafetySupport = Mandatory and Supported (type-safety-enabled path).
        /// Requires sections support (Mandatory or Supported).
        /// For Supported implementations both the type-safe path (this test) and the
        /// weak-safety path (<see cref="WeakSafety_MismatchInsideSection_NoInvalidDataException"/>)
        /// are verified.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task TypeSafety_MismatchIsImmediate_NotDeferredToEndSection()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableTypeSafety(writer, reader);
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.Writer.WriteInt(42);
            writer.EndSection();
            await CompleteFetch(writer, reader);

            reader.BeginSection();

            bool exceptionOnRead = false;
            try { reader.Reader.ReadFloat(); }  // mismatch: wrote Int, reading Float
            catch (InvalidDataException) { exceptionOnRead = true; }

            Assert.That(exceptionOnRead, Is.True,
                "InvalidDataException must be thrown by ReadFloat(), not deferred to EndSection().");
        }

        // ════════════════════════════════════════════════════════════════════════
        // WEAK SAFETY (TypeSafetySupport == NotSupported, or Supported with safety disabled)
        // Applies to: NotSupported (always disabled) and Supported (explicitly disabled
        // via TrySetTypeSafety(false)). Skipped for Mandatory (type safety cannot be disabled).
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// When type safety is disabled and sections are supported, the reader's EndSection()
        /// must report (return false) when bytes inside the section were not fully consumed.
        /// This is the minimum weak-safety guarantee.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_Sections_UnderConsumption_EndSectionReturnsFalse()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.Writer.WriteInt(1);
            writer.Writer.WriteInt(2);  // will not be consumed
            writer.EndSection();
            await CompleteFetch(writer, reader);

            reader.BeginSection();
            reader.Reader.ReadInt();   // consume only one
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
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.Writer.WriteInt(42);
            writer.EndSection();
            await CompleteFetch(writer, reader);

            reader.BeginSection();
            reader.Reader.ReadInt();
            Assert.Throws<EndOfStreamException>(() => reader.Reader.ReadInt(),
                "Reading beyond section boundary must throw EndOfStreamException (weak safety).");
        }

        /// <summary>Exact consumption → EndSection returns true (correct alignment).</summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_Sections_ExactConsumption_EndSectionReturnsTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.Writer.WriteInt(7);
            writer.EndSection();
            await CompleteFetch(writer, reader);

            reader.BeginSection();
            reader.Reader.ReadInt();
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True);
        }

        /// <summary>
        /// When type safety is disabled and sections are NOT supported, reading past the end of the
        /// available data must throw (EndOfStreamException or similar).
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_NoSections_ReadBeyondEnd_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            if (writer.SectionsSupport != CapabilityState.NotSupported)
                Assert.Ignore("This test targets section-less weak-safety (SectionsSupport must be NotSupported).");

            writer.Writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            reader.Reader.ReadInt();  // consume the only int

            Assert.Throws<EndOfStreamException>(() => reader.Reader.ReadInt(),
                "Reading past the end must throw EndOfStreamException in weak-safety mode.");
        }

        /// <summary>
        /// In weak-safety mode, a type mismatch inside a section does NOT throw
        /// <see cref="InvalidDataException"/> — mismatches are undetected (silent).
        /// Runs for TypeSafetySupport = NotSupported and Supported (disabled path).
        /// Requires sections support (Mandatory or Supported).
        /// This is the weak-safety counterpart of
        /// <see cref="TypeSafety_MismatchIsImmediate_NotDeferredToEndSection"/>.
        /// </summary>
        [Test, Category("TypeSafety")]
        public async Task WeakSafety_MismatchInsideSection_NoInvalidDataException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureTypeSafetyDisabled(writer, reader);
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.Writer.WriteInt(42);
            writer.EndSection();
            await CompleteFetch(writer, reader);

            reader.BeginSection();
            bool threwInvalidData = false;
            try { reader.Reader.ReadFloat(); }  // type mismatch — must NOT throw InvalidDataException in weak-safety mode
            catch (InvalidDataException) { threwInvalidData = true; }
            catch (Exception) { /* EndOfStreamException or similar is acceptable */ }

            Assert.That(threwInvalidData, Is.False,
                "In weak-safety mode, type mismatches must NOT throw InvalidDataException (mismatches are undetected).");
        }
    }
}

