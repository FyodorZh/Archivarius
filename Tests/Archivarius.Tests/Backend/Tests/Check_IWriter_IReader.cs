using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    /// <summary>
    /// Abstract NUnit fixture that verifies the full <see cref="IWriterBackend"/>/<see cref="IReaderBackend"/> contract.
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
    ///     protected override Task&lt;(IWriter, IReader)&gt; GetWriterReaderPair()
    ///         =&gt; Task.FromResult&lt;(IWriter, IReader)&gt;(
    ///             (new MyWriter(), new MyReader()));
    /// }
    /// </code>
    /// </para>
    /// <para>Capability flags (both read directly from the returned writer/reader):</para>
    /// <list type="bullet">
    ///   <item><term><see cref="IAnyBackend.SectionsSupport"/></term><description>
    ///     Controls which section-related test paths execute:
    ///     <see cref="CapabilityState.NotSupported"/>: only disabled-sections tests run.
    ///     <see cref="CapabilityState.Mandatory"/>: only enabled-sections tests run (sections always on).
    ///     <see cref="CapabilityState.Supported"/>: both paths run via <c>TrySetSectionUsage</c>.
    ///   </description></item>
    ///   <item><term><see cref="IAnyBackend.TypeSafetySupport"/></term><description>
    ///     Controls which type-safety test paths execute:
    ///     <see cref="CapabilityState.NotSupported"/>: only weak-safety tests run.
    ///     <see cref="CapabilityState.Mandatory"/>: only type-safe tests run (type safety always on).
    ///     <see cref="CapabilityState.Supported"/>: both paths run via <c>TrySetTypeSafety</c>.
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
        /// available to the reader after the matching <see cref="IReaderBackend.Fetch"/> call.
        /// </summary>
        /// <remarks>
        /// Capability flags (<see cref="IAnyBackend.SectionsSupport"/>,
        /// <see cref="IAnyBackend.TypeSafetySupport"/>) are determined by reading the
        /// corresponding properties on the returned objects.
        /// Both writer and reader always return the same value for each property.
        /// </remarks>
        protected abstract Task<(IWriterBackend writer, IReaderBackend reader)>
            GetWriterReaderPair();

        // ── Shared constants ─────────────────────────────────────────────────────

        protected static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        // ── Shared helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Awaits <see cref="IWriterBackend.Flush"/> then awaits <see cref="IReaderBackend.Fetch"/> with a
        /// 5-second timeout. Asserts that Fetch returns <see langword="true"/>.
        /// </summary>
        protected static async Task FlushFetch(IWriterBackend writer, IReaderBackend reader)
        {
            await writer.Flush();
            bool ok = await reader.Fetch().AsTask().WaitAsync(TestTimeout);
            Assert.That(ok, Is.True, "Fetch() must return true after a matching Flush().");
        }

        /// <summary>
        /// Awaits <see cref="IWriterBackend.Complete"/> then awaits <see cref="IReaderBackend.Fetch"/> with a
        /// 5-second timeout. Asserts that Fetch returns <see langword="true"/>.
        /// </summary>
        protected static async Task CompleteFetch(IWriterBackend writer, IReaderBackend reader)
        {
            await writer.Complete();
            bool ok = await reader.Fetch().AsTask().WaitAsync(TestTimeout);
            Assert.That(ok, Is.True, "Fetch() must return true after a matching Complete().");
        }

        /// <summary>Awaits <see cref="IReaderBackend.Fetch"/> with a 5-second timeout.</summary>
        protected static Task<bool> FetchWithTimeout(IReaderBackend reader)
            => reader.Fetch().AsTask().WaitAsync(TestTimeout);

        /// <summary>
        /// Ensures sections are ENABLED on both writer and reader for the current test.
        /// The required capability is read from <see cref="IAnyBackend.SectionsSupport"/>
        /// on the <paramref name="writer"/>.
        /// <list type="bullet">
        ///   <item><see cref="CapabilityState.NotSupported"/>:
        ///     calls <c>Assert.Ignore</c> — sections cannot be enabled.</item>
        ///   <item><see cref="CapabilityState.Mandatory"/>:
        ///     no-op — sections are already permanently active.</item>
        ///   <item><see cref="CapabilityState.Supported"/>:
        ///     calls <c>TrySetSectionUsage(true)</c> on both writer and reader.</item>
        /// </list>
        /// Must be called before any data is written or read.
        /// </summary>
        protected static void EnableSections(IWriterBackend writer, IReaderBackend reader)
        {
            switch (writer.SectionsSupport)
            {
                case CapabilityState.NotSupported:
                    Assert.Ignore("This implementation does not support sections (SectionsSupport = NotSupported).");
                    break;
                case CapabilityState.Mandatory:
                    // Sections are permanently active — nothing to do.
                    break;
                case CapabilityState.Supported:
                    if (!writer.TrySetSectionUsage(true))
                        Assert.Ignore("TrySetSectionUsage(true) returned false unexpectedly on writer.");
                    if (!reader.TrySetSectionUsage(true))
                        Assert.Ignore("TrySetSectionUsage(true) returned false unexpectedly on reader.");
                    break;
            }
        }

        /// <summary>
        /// Ensures sections are DISABLED on both writer and reader for the current test.
        /// The required capability is read from <see cref="IAnyBackend.SectionsSupport"/>
        /// on the <paramref name="writer"/>.
        /// <list type="bullet">
        ///   <item><see cref="CapabilityState.NotSupported"/>:
        ///     no-op — sections are always off.</item>
        ///   <item><see cref="CapabilityState.Mandatory"/>:
        ///     calls <c>Assert.Ignore</c> — sections cannot be disabled.</item>
        ///   <item><see cref="CapabilityState.Supported"/>:
        ///     calls <c>TrySetSectionUsage(false)</c> on both writer and reader.
        ///     Fails the test if the call is rejected (must be invoked before any I/O).</item>
        /// </list>
        /// Must be called before any data is written or read.
        /// </summary>
        protected static void EnsureSectionsDisabled(IWriterBackend writer, IReaderBackend reader)
        {
            switch (writer.SectionsSupport)
            {
                case CapabilityState.NotSupported:
                    // Already disabled — nothing to do.
                    break;
                case CapabilityState.Mandatory:
                    Assert.Ignore("Sections are Mandatory for this implementation and cannot be disabled; test skipped.");
                    break;
                case CapabilityState.Supported:
                    if (!writer.TrySetSectionUsage(false))
                        Assert.Fail("TrySetSectionUsage(false) returned false for a Supported writer before any I/O.");
                    if (!reader.TrySetSectionUsage(false))
                        Assert.Fail("TrySetSectionUsage(false) returned false for a Supported reader before any I/O.");
                    break;
            }
        }

        /// <summary>
        /// Ensures type safety is ENABLED on both writer and reader for the current test.
        /// The required capability is read from <see cref="IAnyBackend.TypeSafetySupport"/>
        /// on the <paramref name="writer"/>.
        /// <list type="bullet">
        ///   <item><see cref="CapabilityState.NotSupported"/>:
        ///     calls <c>Assert.Ignore</c> — type safety cannot be enabled.</item>
        ///   <item><see cref="CapabilityState.Mandatory"/>:
        ///     no-op — type safety is already permanently active.</item>
        ///   <item><see cref="CapabilityState.Supported"/>:
        ///     calls <c>TrySetTypeSafety(true)</c> on both writer and reader.</item>
        /// </list>
        /// Must be called before any data is written or read.
        /// </summary>
        protected static void EnableTypeSafety(IWriterBackend writer, IReaderBackend reader)
        {
            switch (writer.TypeSafetySupport)
            {
                case CapabilityState.NotSupported:
                    Assert.Ignore("This implementation does not support type safety (TypeSafetySupport = NotSupported).");
                    break;
                case CapabilityState.Mandatory:
                    // Type safety is permanently active — nothing to do.
                    break;
                case CapabilityState.Supported:
                    if (!writer.TrySetTypeSafety(true))
                        Assert.Ignore("TrySetTypeSafety(true) returned false unexpectedly on writer.");
                    if (!reader.TrySetTypeSafety(true))
                        Assert.Ignore("TrySetTypeSafety(true) returned false unexpectedly on reader.");
                    break;
            }
        }

        /// <summary>
        /// Ensures type safety is DISABLED on both writer and reader for the current test.
        /// The required capability is read from <see cref="IAnyBackend.TypeSafetySupport"/>
        /// on the <paramref name="writer"/>.
        /// <list type="bullet">
        ///   <item><see cref="CapabilityState.NotSupported"/>:
        ///     no-op — type safety is always off.</item>
        ///   <item><see cref="CapabilityState.Mandatory"/>:
        ///     calls <c>Assert.Ignore</c> — type safety cannot be disabled.</item>
        ///   <item><see cref="CapabilityState.Supported"/>:
        ///     calls <c>TrySetTypeSafety(false)</c> on both writer and reader.
        ///     Fails the test if the call is rejected (must be invoked before any I/O).</item>
        /// </list>
        /// Must be called before any data is written or read.
        /// </summary>
        protected static void EnsureTypeSafetyDisabled(IWriterBackend writer, IReaderBackend reader)
        {
            switch (writer.TypeSafetySupport)
            {
                case CapabilityState.NotSupported:
                    // Already disabled — nothing to do.
                    break;
                case CapabilityState.Mandatory:
                    Assert.Ignore("Type safety is Mandatory for this implementation and cannot be disabled; test skipped.");
                    break;
                case CapabilityState.Supported:
                    if (!writer.TrySetTypeSafety(false))
                        Assert.Fail("TrySetTypeSafety(false) returned false for a Supported writer before any I/O.");
                    if (!reader.TrySetTypeSafety(false))
                        Assert.Fail("TrySetTypeSafety(false) returned false for a Supported reader before any I/O.");
                    break;
            }
        }

        // ── Defaults ─────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>TrySetSectionUsage(false)</c> must succeed (return <see langword="true"/>) for
        /// <see cref="CapabilityState.NotSupported"/> and
        /// <see cref="CapabilityState.Supported"/> implementations.
        /// For <see cref="CapabilityState.Mandatory"/> it must return
        /// <see langword="false"/> because disabling mandatory sections is not allowed.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetSectionUsageFalse_MatchesSectionsSupport()
        {
            var (writer, _) = await GetWriterReaderPair();
            bool expected = writer.SectionsSupport != CapabilityState.Mandatory;
            Assert.That(writer.TrySetSectionUsage(false), Is.EqualTo(expected),
                $"TrySetSectionUsage(false) expected {expected} for SectionsSupport={writer.SectionsSupport}.");
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetSectionUsageFalse_MatchesSectionsSupport()
        {
            var (_, reader) = await GetWriterReaderPair();
            bool expected = reader.SectionsSupport != CapabilityState.Mandatory;
            Assert.That(reader.TrySetSectionUsage(false), Is.EqualTo(expected),
                $"TrySetSectionUsage(false) expected {expected} for SectionsSupport={reader.SectionsSupport}.");
        }

        /// <summary>
        /// <c>TrySetSectionUsage(true)</c> must succeed (return <see langword="true"/>) for
        /// <see cref="CapabilityState.Supported"/> and
        /// <see cref="CapabilityState.Mandatory"/> implementations.
        /// For <see cref="CapabilityState.NotSupported"/> it must return
        /// <see langword="false"/> because enabling unsupported sections is not allowed.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetSectionUsageTrue_MatchesSectionsSupport()
        {
            var (writer, _) = await GetWriterReaderPair();
            bool expected = writer.SectionsSupport != CapabilityState.NotSupported;
            Assert.That(writer.TrySetSectionUsage(true), Is.EqualTo(expected),
                $"TrySetSectionUsage(true) expected {expected} for SectionsSupport={writer.SectionsSupport}.");
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetSectionUsageTrue_MatchesSectionsSupport()
        {
            var (_, reader) = await GetWriterReaderPair();
            bool expected = reader.SectionsSupport != CapabilityState.NotSupported;
            Assert.That(reader.TrySetSectionUsage(true), Is.EqualTo(expected),
                $"TrySetSectionUsage(true) expected {expected} for SectionsSupport={reader.SectionsSupport}.");
        }

        /// <summary>
        /// <c>TrySetTypeSafety(false)</c> must succeed (return <see langword="true"/>) for
        /// <see cref="CapabilityState.NotSupported"/> and
        /// <see cref="CapabilityState.Supported"/> implementations.
        /// For <see cref="CapabilityState.Mandatory"/> it must return
        /// <see langword="false"/> because disabling mandatory type safety is not allowed.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetTypeSafetyFalse_MatchesTypeSafetySupport()
        {
            var (writer, _) = await GetWriterReaderPair();
            bool expected = writer.TypeSafetySupport != CapabilityState.Mandatory;
            Assert.That(writer.TrySetTypeSafety(false), Is.EqualTo(expected),
                $"TrySetTypeSafety(false) expected {expected} for TypeSafetySupport={writer.TypeSafetySupport}.");
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetTypeSafetyFalse_MatchesTypeSafetySupport()
        {
            var (_, reader) = await GetWriterReaderPair();
            bool expected = reader.TypeSafetySupport != CapabilityState.Mandatory;
            Assert.That(reader.TrySetTypeSafety(false), Is.EqualTo(expected),
                $"TrySetTypeSafety(false) expected {expected} for TypeSafetySupport={reader.TypeSafetySupport}.");
        }

        /// <summary>
        /// <c>TrySetTypeSafety(true)</c> must succeed (return <see langword="true"/>) for
        /// <see cref="CapabilityState.Supported"/> and
        /// <see cref="CapabilityState.Mandatory"/> implementations.
        /// For <see cref="CapabilityState.NotSupported"/> it must return
        /// <see langword="false"/> because enabling unsupported type safety is not allowed.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_TrySetTypeSafetyTrue_MatchesTypeSafetySupport()
        {
            var (writer, _) = await GetWriterReaderPair();
            bool expected = writer.TypeSafetySupport != CapabilityState.NotSupported;
            Assert.That(writer.TrySetTypeSafety(true), Is.EqualTo(expected),
                $"TrySetTypeSafety(true) expected {expected} for TypeSafetySupport={writer.TypeSafetySupport}.");
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_TrySetTypeSafetyTrue_MatchesTypeSafetySupport()
        {
            var (_, reader) = await GetWriterReaderPair();
            bool expected = reader.TypeSafetySupport != CapabilityState.NotSupported;
            Assert.That(reader.TrySetTypeSafety(true), Is.EqualTo(expected),
                $"TrySetTypeSafety(true) expected {expected} for TypeSafetySupport={reader.TypeSafetySupport}.");
        }

        /// <summary>IsCompleted must be false before any Fetch() call.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Reader_IsCompleted_FalseBeforeAnyFetch()
        {
            var (_, reader) = await GetWriterReaderPair();
            Assert.That(reader.IsCompleted, Is.False);
        }

        /// <summary>Complete() counts as one Flush(). The very next Fetch() must return true.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Fetch_ReturnsTrueAfterComplete()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteInt(1);
            await writer.Complete();
            bool first = await FetchWithTimeout(reader);
            Assert.That(first, Is.True, "First Fetch() after Complete() must return true.");
        }

        /// <summary>After the data Fetch, the terminal Fetch must return false.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Fetch_ReturnsFalseAfterDataConsumedAndComplete()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.Reader.ReadInt();
            bool second = await FetchWithTimeout(reader);
            Assert.That(second, Is.False, "Fetch() after Complete() and data consumed must return false.");
        }

        /// <summary>IsCompleted flips to true only once Fetch() returns false.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_IsCompleted_TrueAfterTerminalFetch()
        { // something wrong here!!!
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.Reader.ReadInt();
            Assert.That(reader.IsCompleted, Is.False, "IsCompleted must be false before the terminal Fetch.");
            await FetchWithTimeout(reader);
            Assert.That(reader.IsCompleted, Is.True);
        }

        /// <summary>EndSection() without a prior BeginSection always throws InvalidOperationException on the reader.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Reader_EndSection_WithoutBeginSection_ThrowsInvalidOp()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }

        /// <summary>EndSection() without a prior BeginSection always throws InvalidOperationException on the writer.</summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_EndSection_WithoutBeginSection_ThrowsInvalidOp()
        {
            var (writer, _) = await GetWriterReaderPair();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        /// <summary>
        /// For <see cref="CapabilityState.NotSupported"/> implementations:
        /// BeginSection/EndSection must be no-ops that inject no extra bytes, so the reader can
        /// consume values without matching Begin/End calls.
        /// This test only applies to <c>NotSupported</c> because that is the only mode with a
        /// well-defined "always disabled" default. For <c>Supported</c> the explicit-disable path is
        /// covered by <c>Sections_Disabled_*</c> tests; for <c>Mandatory</c> sections are always on.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_SectionsDisabledByDefault_BeginEndAreNoop_BehavioralCheck()
        {
            var (writer, reader) = await GetWriterReaderPair();
            if (writer.SectionsSupport != CapabilityState.NotSupported)
                Assert.Ignore(
                    "This test verifies default-disabled (no TrySetSectionUsage call) behavior; " +
                    "only applicable to NotSupported implementations.");

            // Writer: BeginSection (no-op), write two ints, EndSection (no-op)
            writer.BeginSection();
            writer.Writer.WriteInt(42);
            writer.Writer.WriteInt(99);
            writer.EndSection();
            await CompleteFetch(writer, reader);
            // Reader reads without any section calls — if sections injected a length header,
            // ReadInt() would return the header bytes instead of 42.
            Assert.That(reader.Reader.ReadInt(), Is.EqualTo(42));
            Assert.That(reader.Reader.ReadInt(), Is.EqualTo(99));
        }

        /// <summary>
        /// ExtraEndSection after a balanced Begin/End pair still throws InvalidOperationException.
        /// </summary>
        [Test, Category("Defaults")]
        public async Task Defaults_Writer_ExtraEndSection_AfterBalancedPair_Throws()
        {
            var (writer, _) = await GetWriterReaderPair();
            writer.BeginSection();
            writer.Writer.WriteInt(1);
            writer.EndSection();
            Assert.Throws<InvalidOperationException>(() => writer.EndSection());
        }

        [Test, Category("Defaults")]
        public async Task Defaults_Reader_ExtraEndSection_AfterBalancedPair_Throws()
        {
            var (writer, reader) = await GetWriterReaderPair();
            writer.Writer.WriteInt(1);
            await CompleteFetch(writer, reader);
            reader.BeginSection();
            reader.EndSection();
            Assert.Throws<InvalidOperationException>(() => reader.EndSection());
        }
    }
}