using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ────────────────────────────────────────────────────────────────────────
        // Helper: drive the writer to a "completed" state and return it.
        // ────────────────────────────────────────────────────────────────────────

        private async Task<(IWriter writer, IReader reader)> GetCompletedWriter()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            // Drain reader to avoid blocking transports
            bool got = await FetchWithTimeout(reader);
            if (got) reader.ReadInt();
            return (writer, reader);
        }

        // ════════════════════════════════════════════════════════════════════════
        // POST-COMPLETE ERRORS — all Write* methods must throw
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteBool_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteBool(true));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteByte_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteByte(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteSByte_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteSByte(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteChar_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteChar('a'));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteShort_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteShort(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteUShort_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteUShort(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteInt_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteInt(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteUInt_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteUInt(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteLong_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteLong(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteULong_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteULong(0));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteFloat_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteFloat(0f));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteDouble_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteDouble(0d));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteDecimal_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteDecimal(0m));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteString_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteString("x"));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteArray_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteArray(new byte[] { 1 }));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_WriteBytes_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.WriteBytes(new byte[] { 1 }, 0, 1));
        }

        // ── Post-complete control operations ──────────────────────────────────────

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_BeginSection_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.BeginSection());
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_EndSection_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_TrySetSectionUsageTrue_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            // TrySetSectionUsage(true) after Complete() must throw (writer is completed)
            Assert.Throws<InvalidOperationException>(() => writer.TrySetSectionUsage(true));
        }

        [Test, Category("Errors")]
        public async Task Errors_PostComplete_Flush_Throws()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Flush());
        }

        /// <summary>Calling Complete() a second time must throw InvalidOperationException.</summary>
        [Test, Category("Errors")]
        public async Task Errors_PostComplete_Complete_SecondCallThrows()
        {
            var (writer, _) = await GetCompletedWriter();
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Complete());
        }

        // ════════════════════════════════════════════════════════════════════════
        // TrySetSectionUsage after activity
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Errors")]
        public async Task Errors_TrySetSectionUsage_AfterDataWritten_ReturnsFalse()
        {
            var (writer, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            Assert.That(writer.TrySetSectionUsage(false), Is.False,
                "TrySetSectionUsage must return false after any data has been written.");
        }

        [Test, Category("Errors")]
        public async Task Errors_TrySetSectionUsage_AfterDataRead_ReturnsFalse()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            reader.ReadInt();
            Assert.That(reader.TrySetSectionUsage(false), Is.False,
                "TrySetSectionUsage must return false after any data has been read.");
        }

        [Test, Category("Errors")]
        public async Task Errors_TrySetSectionUsage_AfterBeginSection_ReturnsFalse()
        {
            // When sections ARE active, BeginSection() writes infrastructure bytes to the stream.
            // Any further TrySetSectionUsage() call must then return false.
            var (writer, _) = await GetWriterReaderPair();
            if (writer.SectionsSupport == CapabilityState.NotSupported)
                Assert.Ignore("BeginSection is a no-op when sections are not supported; no bytes are written.");

            // For Mandatory, sections are always active — no TrySetSectionUsage(true) needed.
            // For Supported, we must explicitly enable first.
            if (writer.SectionsSupport == CapabilityState.Supported)
                writer.TrySetSectionUsage(true);

            writer.BeginSection();  // writes a length-header placeholder
            Assert.That(writer.TrySetSectionUsage(false), Is.False,
                "TrySetSectionUsage must return false after BeginSection() has written infrastructure bytes.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // Section nesting errors
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Errors")]
        public async Task Errors_Writer_ExtraEndSection_AfterAllClosed_Throws()
        {
            var (writer, _) = await GetWriterReaderPair();
            writer.BeginSection();
            writer.WriteInt(1);
            writer.EndSection();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection(),
                "EndSection() called one time too many must throw.");
        }

        [Test, Category("Errors")]
        public async Task Errors_Reader_ExtraEndSection_AfterAllClosed_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            reader.BeginSection();
            reader.EndSection();
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }

        /// <summary>Flush() with an open section when sections are ENABLED must throw.</summary>
        [Test, Category("Errors")]
        public async Task Errors_Sections_Enabled_Flush_WithOpenSection_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Flush(),
                "Flush() inside an open section (sections enabled) must throw.");
        }

        /// <summary>Complete() with an open section when sections are ENABLED must throw.</summary>
        [Test, Category("Errors")]
        public async Task Errors_Sections_Enabled_Complete_WithOpenSection_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Complete());
        }

        // ════════════════════════════════════════════════════════════════════════
        // End-of-stream reads
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Errors")]
        public async Task Errors_Read_BeyondStreamEnd_ReadInt_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            reader.ReadInt();
            // No more data — next read must throw
            Assert.Throws<EndOfStreamException>(() => reader.ReadInt(),
                "ReadInt() with no available data must throw EndOfStreamException.");
        }

        [Test, Category("Errors")]
        public async Task Errors_Read_BeyondStreamEnd_ReadString_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteString("x");
            await CompleteFetch(writer, reader);
            reader.ReadString();
            Assert.Throws<EndOfStreamException>(() => reader.ReadString());
        }

        [Test, Category("Errors")]
        public async Task Errors_Read_BeyondStreamEnd_ReadBool_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteBool(true);
            await CompleteFetch(writer, reader);
            reader.ReadBool();
            Assert.Throws<EndOfStreamException>(() => reader.ReadBool());
        }

        [Test, Category("Errors")]
        public async Task Errors_Read_BeyondStreamEnd_ReadArray_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteArray(new byte[] { 1 });
            await CompleteFetch(writer, reader);
            reader.ReadArray();
            Assert.Throws<EndOfStreamException>(() => reader.ReadArray());
        }

        /// <summary>BeginSection() on a completed reader (IsCompleted=true) must throw.</summary>
        [Test, Category("Errors")]
        public async Task Errors_Reader_BeginSection_WhenIsCompleted_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.ReadInt();
            await FetchWithTimeout(reader);  // terminal → IsCompleted = true

            Assert.That(reader.IsCompleted, Is.True);
            Assert.Throws<InvalidOperationException>(() => reader.BeginSection(),
                "BeginSection() on a completed reader must throw.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // Sections-disabled — nesting LIFO still enforced
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Errors")]
        public async Task Errors_Sections_Disabled_NestedEndSection_WrongOrder_Throws()
        {
            // Even with sections disabled, you can't call EndSection() one time too many.
            var (writer, _) = await GetWriterReaderPair();
            writer.BeginSection();       // depth = 1
            writer.BeginSection();       // depth = 2
            writer.EndSection();         // depth = 1
            writer.EndSection();         // depth = 0
            Assert.Throws<InvalidOperationException>(() => writer.EndSection()); // depth = -1 → error
        }

        /// <summary>
        /// Verifies that after completing the writer, TrySetSectionUsage behavior is consistent.
        /// The writer must have already been completed — so TrySetSectionUsage is called on a
        /// closed writer, which must throw.
        /// </summary>
        [Test, Category("Errors")]
        public async Task Errors_PostComplete_TrySetSectionUsageFalse_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            // Drain to unblock transport if needed
            bool got = await FetchWithTimeout(reader);
            if (got) reader.ReadInt();

            Assert.Throws<InvalidOperationException>(() => writer.TrySetSectionUsage(false),
                "All operations on a completed writer must throw InvalidOperationException.");
        }
    }
}

