using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ════════════════════════════════════════════════════════════════════════
        // SECTIONS DISABLED
        // Applies to: NotSupported (always disabled) and Supported (explicitly disabled
        // via TrySetSectionUsage(false)). Skipped for Mandatory (sections cannot be disabled).
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// With sections disabled, BeginSection/EndSection must not inject any bytes into the stream;
        /// data written inside a section must be readable without matching Begin/End on the reader.
        /// </summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_DataIntegrityPreserved_BeginEndAreNoop()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureSectionsDisabled(writer, reader);

            writer.BeginSection();
            writer.WriteInt(100);
            writer.WriteInt(200);
            writer.EndSection();
            await FlushFetch(writer, reader);
            // Reader reads without matching BeginSection — must work if sections are truly disabled
            Assert.That(reader.ReadInt(), Is.EqualTo(100));
            Assert.That(reader.ReadInt(), Is.EqualTo(200));
        }

        /// <summary>EndSection() with matching BeginSection must return true when sections are disabled.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_EndSection_ReturnsTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureSectionsDisabled(writer, reader);

            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            reader.BeginSection();
            reader.ReadInt();
            bool result = reader.EndSection();
            Assert.That(result, Is.True,
                "EndSection() must return true when sections are disabled (no boundary tracking).");
        }

        /// <summary>Flush() with an open section must NOT throw when sections are disabled.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_Flush_WithOpenSection_DoesNotThrow()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureSectionsDisabled(writer, reader);

            writer.BeginSection();
            writer.WriteInt(42);
            // Flush while section is open — no throw expected when sections are disabled
            Assert.DoesNotThrowAsync(async () => await writer.Flush());
        }

        /// <summary>Complete() with an open section must throw InvalidOperationException (LIFO nesting is always enforced).</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_Complete_WithOpenSection_DoesThrow()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureSectionsDisabled(writer, reader);

            writer.BeginSection();
            writer.WriteInt(42);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Complete());
        }

        /// <summary>Two levels of nesting must work correctly (LIFO) when sections are disabled.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_NestedTwoLevels_Works()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnsureSectionsDisabled(writer, reader);

            writer.BeginSection();    // L1
            writer.BeginSection();    // L2
            writer.WriteInt(7);
            writer.EndSection();      // L2
            writer.WriteInt(8);
            writer.EndSection();      // L1
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(7));
            Assert.That(reader.ReadInt(), Is.EqualTo(8));
        }

        /// <summary>Writer: EndSection() without BeginSection always throws (LIFO nesting enforced regardless of sections mode).</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_Writer_EndSection_WithoutBeginSection_Throws()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        /// <summary>Reader: EndSection() without BeginSection always throws (LIFO nesting enforced regardless of sections mode).</summary>
        [Test, Category("Sections")]
        public async Task Sections_Disabled_Reader_EndSection_WithoutBeginSection_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }

        // ════════════════════════════════════════════════════════════════════════
        // SECTIONS ENABLED
        // Applies to: Mandatory (always enabled) and Supported (explicitly enabled
        // via TrySetSectionUsage(true)). Skipped for NotSupported (sections not available).
        // ════════════════════════════════════════════════════════════════════════

        [Test, Category("Sections")]
        public async Task Sections_Enabled_EmptySection_ReturnsTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True, "Empty section fully consumed → EndSection must return true.");
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_SingleValue_FullConsumption_ReturnsTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(123);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            Assert.That(reader.ReadInt(), Is.EqualTo(123));
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True);
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_MultipleValues_FullConsumption_ReturnsTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            writer.WriteInt(2);
            writer.WriteInt(3);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            Assert.That(reader.ReadInt(), Is.EqualTo(1));
            Assert.That(reader.ReadInt(), Is.EqualTo(2));
            Assert.That(reader.ReadInt(), Is.EqualTo(3));
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True);
        }

        /// <summary>
        /// When reader consumes less than the section contained, EndSection() must skip remaining
        /// bytes and return false.
        /// </summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_PartialConsumption_EndSection_ReturnsFalse()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(10);
            writer.WriteInt(20);   // will NOT be read
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            Assert.That(reader.ReadInt(), Is.EqualTo(10));
            bool skipped = reader.EndSection();
            Assert.That(skipped, Is.False, "EndSection must return false when bytes are skipped.");
        }

        /// <summary>After EndSection() skips bytes the next read must work correctly (stream alignment).</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_PartialConsumption_DataAfterSection_StillReadable()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(10);
            writer.WriteInt(20);  // skipped inside section
            writer.EndSection();
            writer.WriteInt(999); // written outside section
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();     // consume 10
            reader.EndSection();  // skip 20, restore boundary

            Assert.That(reader.ReadInt(), Is.EqualTo(999), "Data after section must be readable after EndSection skip.");
        }

        /// <summary>Read*() beyond section boundary must throw EndOfStreamException.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_OverConsumption_ThrowsEndOfStreamException()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(42);   // 4 bytes in section
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            Assert.That(reader.ReadInt(), Is.EqualTo(42));
            // Section is now exhausted — any further read must throw
            Assert.Throws<EndOfStreamException>(() => reader.ReadInt(),
                "Reading beyond the section boundary must throw EndOfStreamException.");
        }

        /// <summary>
        /// Confirms that the exception is thrown by Read*() itself, not deferred to EndSection().
        /// </summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_OverConsumption_ExceptionImmediateNotDeferred()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();

            bool exceptionFromRead = false;
            try { reader.ReadBool(); }
            catch (EndOfStreamException) { exceptionFromRead = true; }

            Assert.That(exceptionFromRead, Is.True,
                "EndOfStreamException must come from the Read*() call, not deferred to EndSection().");
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_NestedSections_BothFullyConsumed_BothReturnTrue()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();         // outer
            writer.WriteInt(10);
            writer.BeginSection();         // inner
            writer.WriteInt(20);
            writer.EndSection();           // inner
            writer.WriteInt(30);
            writer.EndSection();           // outer
            await FlushFetch(writer, reader);

            reader.BeginSection();         // outer
            Assert.That(reader.ReadInt(), Is.EqualTo(10));
            reader.BeginSection();         // inner
            Assert.That(reader.ReadInt(), Is.EqualTo(20));
            bool innerOk = reader.EndSection();  // inner
            Assert.That(reader.ReadInt(), Is.EqualTo(30));
            bool outerOk = reader.EndSection();  // outer

            Assert.That(innerOk, Is.True, "Inner section fully consumed.");
            Assert.That(outerOk, Is.True, "Outer section fully consumed.");
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_NestedSections_InnerPartial_OuterFullyConsumed()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();         // outer
            writer.BeginSection();         // inner: writes 2 ints
            writer.WriteInt(1);
            writer.WriteInt(2);
            writer.EndSection();           // inner
            writer.WriteInt(3);
            writer.EndSection();           // outer
            await FlushFetch(writer, reader);

            reader.BeginSection();         // outer
            reader.BeginSection();         // inner
            Assert.That(reader.ReadInt(), Is.EqualTo(1));
            bool innerSkipped = reader.EndSection(); // inner: skips int(2)
            Assert.That(innerSkipped, Is.False);
            Assert.That(reader.ReadInt(), Is.EqualTo(3));
            bool outerOk = reader.EndSection();
            Assert.That(outerOk, Is.True);
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_NestedSections_ThreeLevels_AllFull()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.BeginSection();
            writer.BeginSection();
            writer.WriteInt(777);
            writer.EndSection();
            writer.EndSection();
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.BeginSection();
            reader.BeginSection();
            Assert.That(reader.ReadInt(), Is.EqualTo(777));
            Assert.That(reader.EndSection(), Is.True);
            Assert.That(reader.EndSection(), Is.True);
            Assert.That(reader.EndSection(), Is.True);
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_MultipleSequentialSections_AllCorrect()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            for (int i = 0; i < 5; i++)
            {
                writer.BeginSection();
                writer.WriteInt(i * 10);
                writer.EndSection();
            }
            await FlushFetch(writer, reader);

            for (int i = 0; i < 5; i++)
            {
                reader.BeginSection();
                Assert.That(reader.ReadInt(), Is.EqualTo(i * 10));
                bool ok = reader.EndSection();
                Assert.That(ok, Is.True, $"Section {i} must be fully consumed.");
            }
        }

        /// <summary>Flush() with an open section (sections enabled) must throw InvalidOperationException.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_Flush_WithOpenSection_ThrowsInvalidOp()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await writer.Flush(),
                "Flush() with an open section (sections enabled) must throw.");
        }

        /// <summary>Complete() with an open section (sections enabled) must throw InvalidOperationException.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_Complete_WithOpenSection_ThrowsInvalidOp()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.Complete());
        }

        /// <summary>Writer: EndSection without BeginSection throws even when sections are enabled.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_Writer_EndSection_WithoutBeginSection_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        /// <summary>Reader: EndSection without BeginSection throws even when sections are enabled.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_Reader_EndSection_WithoutBeginSection_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            reader.ReadInt();
            reader.EndSection();
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }

        [Test, Category("Sections")]
        public async Task Sections_Enabled_LargeSection_FullConsumption()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            const int count = 1000;
            writer.BeginSection();
            for (int i = 0; i < count; i++) writer.WriteInt(i);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            for (int i = 0; i < count; i++)
                Assert.That(reader.ReadInt(), Is.EqualTo(i));
            bool ok = reader.EndSection();
            Assert.That(ok, Is.True);
        }

        /// <summary>TrySetSectionUsage returns false after data has been written.</summary>
        [Test, Category("Sections")]
        public async Task Sections_TrySetSectionUsage_AfterDataWritten_ReturnsFalse()
        {
            var (writer, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            // Now it's too late to change section mode
            Assert.That(writer.TrySetSectionUsage(false), Is.False,
                "TrySetSectionUsage must return false after data has been written.");
        }

        /// <summary>TrySetSectionUsage returns false after data has been read.</summary>
        [Test, Category("Sections")]
        public async Task Sections_TrySetSectionUsage_AfterDataRead_ReturnsFalse()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            reader.ReadInt();
            Assert.That(reader.TrySetSectionUsage(false), Is.False,
                "TrySetSectionUsage must return false after data has been read.");
        }

        /// <summary>BeginSection on reader when IsCompleted=true must throw InvalidOperationException.</summary>
        [Test, Category("Sections")]
        public async Task Sections_Reader_BeginSection_WhenIsCompleted_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader); // data fetch
            reader.ReadInt();
            await FetchWithTimeout(reader); // terminal → IsCompleted = true

            Assert.That(reader.IsCompleted, Is.True);
            Assert.Throws<InvalidOperationException>(() => reader.BeginSection(),
                "BeginSection() must throw when reader IsCompleted.");
        }

        /// <summary>
        /// ReadBytes() that would consume bytes beyond the section boundary must throw EndOfStreamException,
        /// just like any other Read*() call that exceeds the section boundary.
        /// </summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_ReadBytes_BeyondSectionBoundary_ThrowsEndOfStream()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            // Write a section containing exactly 2 bytes
            writer.BeginSection();
            writer.WriteBytes(new byte[] { 0xAA, 0xBB }, 0, 2);
            writer.EndSection();
            await FlushFetch(writer, reader);

            reader.BeginSection();
            // Trying to read 3 bytes when only 2 are available in the section
            byte[] dst = new byte[3];
            Assert.Throws<EndOfStreamException>(() => reader.ReadBytes(dst, 0, 3),
                "ReadBytes beyond section boundary must throw EndOfStreamException.");
        }

        /// <summary>
        /// Fetch() must throw InvalidOperationException when a section is currently open on the reader.
        /// </summary>
        [Test, Category("Sections")]
        public async Task Sections_Enabled_Fetch_WithOpenSection_ThrowsInvalidOp()
        {
            var (writer, reader) = await GetWriterReaderPair();
            EnableSections(writer, reader);

            writer.BeginSection();
            writer.WriteInt(1);
            writer.EndSection();
            await FlushFetch(writer, reader);  // first cycle — read the section header etc.

            // Open a section on the reader side, then try to call Fetch() while it's open
            reader.BeginSection();
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await reader.Fetch(),
                "Fetch() while a section is open must throw InvalidOperationException.");
        }
    }
}

