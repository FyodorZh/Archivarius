using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    /// <summary>
    /// Abstract NUnit fixture that verifies the full <see cref="IWriter"/>/<see cref="IReader"/> contract.
    /// Concrete subclasses supply a fresh connected pair for each test via <see cref="GetWriterReaderPair"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To add a new implementation under test, create a concrete subclass decorated with
    /// <c>[TestFixture]</c> and override <see cref="GetWriterReaderPair"/>:
    /// <code>
    /// [TestFixture]
    /// public class Check_MyWriter_MyReader : Check_IWriter_IReader
    /// {
    ///     protected override Task&lt;(IWriter, IReader, bool, bool)&gt; GetWriterReaderPair()
    ///         =&gt; Task.FromResult&lt;(IWriter, IReader, bool, bool)&gt;(
    ///             (new MyWriter(), new MyReader(), sectionsAreSupported: true, typeSafety: false));
    /// }
    /// </code>
    /// </para>
    /// <para>Capability flags:</para>
    /// <list type="bullet">
    ///   <item><term>sectionsAreSupported</term><description>
    ///     Both writer and reader honour <c>TrySetSectionUsage(true)</c>. Section-specific tests run only when this is <see langword="true"/>.
    ///   </description></item>
    ///   <item><term>typeSafety</term><description>
    ///     Every type mismatch on read immediately throws <see cref="InvalidDataException"/>.
    ///     When <see langword="false"/>, weak-safety tests run instead.
    ///   </description></item>
    /// </list>
    /// </remarks>
    [TestFixture]
    public abstract partial class Check_IWriter_IReader
    {
        // ── Abstract factory ────────────────────────────────────────────────────

        /// <summary>
        /// Produces a fresh writer/reader pair for a single test.
        /// The writer and reader must be connected: data flushed by the writer becomes
        /// available to the reader after the matching <see cref="IReader.Fetch"/> call.
        /// </summary>
        protected abstract Task<(IWriter writer, IReader reader, bool sectionsAreSupported, bool typeSafety)>
            GetWriterReaderPair();

        // ── Shared constants ─────────────────────────────────────────────────────

        protected static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        // ── Shared helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Awaits <see cref="IWriter.Flush"/> then awaits <see cref="IReader.Fetch"/> with a
        /// 5-second timeout. Asserts that Fetch returns <see langword="true"/>.
        /// </summary>
        protected static async Task FlushFetch(IWriter writer, IReader reader)
        {
            await writer.Flush();
            bool ok = await reader.Fetch().AsTask().WaitAsync(TestTimeout);
            Assert.That(ok, Is.True, "Fetch() must return true after a matching Flush().");
        }

        /// <summary>Awaits <see cref="IReader.Fetch"/> with a 5-second timeout.</summary>
        protected static Task<bool> FetchWithTimeout(IReader reader)
            => reader.Fetch().AsTask().WaitAsync(TestTimeout);

        /// <summary>
        /// Enables sections on both writer and reader. Calls <c>Assert.Ignore</c>
        /// if the implementation does not support sections.
        /// </summary>
        protected static void EnableSections(IWriter writer, IReader reader)
        {
            if (!writer.TrySetSectionUsage(true))
                Assert.Ignore("This implementation does not support sections (writer returned false).");
            if (!reader.TrySetSectionUsage(true))
                Assert.Ignore("This implementation does not support sections (reader returned false).");
        }

        // ── Defaults ─────────────────────────────────────────────────────────────

        /// <summary>TrySetSectionUsage(false) must always succeed for every implementation.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetSectionUsageFalse_AlwaysReturnsTrue()
        {
            var (writer, _, _, _) = await GetWriterReaderPair();
            Assert.That(writer.TrySetSectionUsage(false), Is.True,
                "TrySetSectionUsage(false) must always return true — disabling is always valid.");
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetSectionUsageFalse_AlwaysReturnsTrue()
        {
            var (_, reader, _, _) = await GetWriterReaderPair();
            Assert.That(reader.TrySetSectionUsage(false), Is.True);
        }

        /// <summary>TrySetSectionUsage(true) must return the same value as the sectionsAreSupported flag.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetSectionUsageTrue_MatchesSupportsSectionsFlag()
        {
            var (writer, _, sectionsAreSupported, _) = await GetWriterReaderPair();
            Assert.That(writer.TrySetSectionUsage(true), Is.EqualTo(sectionsAreSupported));
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetSectionUsageTrue_MatchesSupportsSectionsFlag()
        {
            var (_, reader, sectionsAreSupported, _) = await GetWriterReaderPair();
            Assert.That(reader.TrySetSectionUsage(true), Is.EqualTo(sectionsAreSupported));
        }

        /// <summary>IsCompleted must be false before any Fetch() call.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Reader_IsCompleted_FalseBeforeAnyFetch()
        {
            var (_, reader, _, _) = await GetWriterReaderPair();
            Assert.That(reader.IsCompleted, Is.False);
        }

        /// <summary>Complete() counts as one Flush(). The very next Fetch() must return true.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Fetch_ReturnsTrueAfterComplete()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            bool first = await FetchWithTimeout(reader);
            Assert.That(first, Is.True, "First Fetch() after Complete() must return true.");
        }

        /// <summary>After the data Fetch, the terminal Fetch must return false.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Fetch_ReturnsFalseAfterDataConsumedAndComplete()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.ReadInt();
            bool second = await FetchWithTimeout(reader);
            Assert.That(second, Is.False, "Fetch() after Complete() and data consumed must return false.");
        }

        /// <summary>IsCompleted flips to true only once Fetch() returns false.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_IsCompleted_TrueAfterTerminalFetch()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.ReadInt();
            Assert.That(reader.IsCompleted, Is.False, "IsCompleted must be false before the terminal Fetch.");
            await FetchWithTimeout(reader);
            Assert.That(reader.IsCompleted, Is.True);
        }

        /// <summary>EndSection() without a prior BeginSection always throws InvalidOperationException on the reader.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Reader_EndSection_WithoutBeginSection_ThrowsInvalidOp()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }

        /// <summary>EndSection() without a prior BeginSection always throws InvalidOperationException on the writer.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_EndSection_WithoutBeginSection_ThrowsInvalidOp()
        {
            var (writer, _, _, _) = await GetWriterReaderPair();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        /// <summary>
        /// Sections are disabled by default. With no TrySetSectionUsage(true) call, BeginSection/EndSection on the
        /// writer must be no-ops that write no extra bytes so the reader can read values without matching
        /// BeginSection/EndSection calls.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_SectionsDisabledByDefault_BeginEndAreNoop_BehavioralCheck()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            // Writer: BeginSection (should be no-op), write two ints, EndSection (no-op)
            writer.BeginSection();
            writer.WriteInt(42);
            writer.WriteInt(99);
            writer.EndSection();
            await FlushFetch(writer, reader);
            // Reader reads without any section calls — if sections had been enabled, the length header
            // would land as the first ReadInt() result, causing the assertion below to fail.
            Assert.That(reader.ReadInt(), Is.EqualTo(42));
            Assert.That(reader.ReadInt(), Is.EqualTo(99));
        }

        /// <summary>
        /// ExtraEndSection after a balanced Begin/End pair still throws InvalidOperationException.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_ExtraEndSection_AfterBalancedPair_Throws()
        {
            var (writer, _, _, _) = await GetWriterReaderPair();
            writer.BeginSection();
            writer.WriteInt(1);
            writer.EndSection();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_ExtraEndSection_AfterBalancedPair_Throws()
        {
            var (writer, reader, _, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            reader.BeginSection();
            reader.EndSection();
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }
    }
}