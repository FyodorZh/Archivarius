using System;

namespace Archivarius
{
    public interface ISectionsCapability
    {
        /// <summary>
        /// Describes how this implementation handles logical section markers
        /// (<see cref="BeginSection"/> / <see cref="EndSection"/>).
        /// </summary>
        /// <value>
        /// <list type="bullet">
        ///   <item>
        ///     <term><see cref="CapabilityState.NotSupported"/></term>
        ///     <description>
        ///       Sections are never active for this implementation.
        ///       <see cref="TrySetSectionUsage"/> always returns
        ///       <see langword="true"/> for <c>false</c> (disable) and
        ///       <see langword="false"/> for <c>true</c> (enable).
        ///       <see cref="BeginSection"/> / <see cref="EndSection"/>
        ///       are no-ops, but the LIFO nesting contract is still enforced.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CapabilityState.Supported"/></term>
        ///     <description>
        ///       Sections can be explicitly enabled or disabled via
        ///       <see cref="TrySetSectionUsage"/> before any data is written or read.
        ///       The initial section-usage state is not defined — callers must always set
        ///       the desired mode explicitly before any I/O.
        ///       Both <c>true</c> and <c>false</c> arguments are accepted and return
        ///       <see langword="true"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CapabilityState.Mandatory"/></term>
        ///     <description>
        ///       Sections are permanently enabled; they cannot be turned off.
        ///       <see cref="TrySetSectionUsage"/> returns <see langword="true"/>
        ///       for <c>true</c> (already enabled) and <see langword="false"/> for
        ///       <c>false</c> (disabling is rejected).
        ///     </description>
        ///   </item>
        /// </list>
        /// </value>
        /// <remarks>
        /// <see cref="IReader"/> and <see cref="IWriter"/> always expose the same value for
        /// this property — it is an implementation-defined constant.
        /// </remarks>
        CapabilityState SectionsSupport { get; }

        /// <summary>
        /// Attempts to enable or disable logical section markers.
        /// When sections are enabled, every <see cref="IWriter.BeginSection"/>/<see cref="IWriter.EndSection"/>
        /// pair embeds a byte-length header that allows detection and skipping of unknown or added
        /// fields, providing forward and backward compatibility.
        /// </summary>
        /// <param name="useSections">
        /// <see langword="true"/> to enable section length headers; <see langword="false"/> to disable them.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the mode was successfully applied.
        /// <see langword="false"/> if the request was rejected — either because data has already
        /// been read or written, or because the implementation does not support the requested mode.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Must be called before any data is read or written to the stream.
        /// The paired <see cref="IReader"/> and <see cref="IWriter"/> must be configured with the same value.
        /// </para>
        /// <para>
        /// An implementation that does not support sections must still accept
        /// <c>TrySetSectionUsage(false)</c> and return <see langword="true"/>, because
        /// disabling sections is always valid. It must return <see langword="false"/> only
        /// when <paramref name="useSections"/> is <see langword="true"/>.
        /// </para>
        /// </remarks>
        bool TrySetSectionUsage(bool useSections);

        /// <summary>
        /// Marks the beginning of a logical section.
        /// When sections are enabled, reserves space in the stream for (writer) or reads the (reader)
        /// section's byte-length header, which is filled in or used to constrain reads to the section's boundary.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Every <see cref="BeginSection"/> call must be balanced by exactly one
        /// <see cref="EndSection"/> call. Sections must be properly nested — i.e. sections
        /// behave like matching brackets: the most recently opened section must be closed first.
        /// </para>
        /// <para>
        /// Can be called regardless of whether sections are enabled via
        /// <see cref="TrySetSectionUsage"/>. When sections are disabled, this method is a no-op,
        /// but the nesting contract must still be respected.
        /// </para>
        /// <para>
        /// When sections are enabled in a reader, any read call that would consume bytes beyond
        /// the current section's recorded length boundary throws <see cref="System.IO.EndOfStreamException"/>,
        /// even if more data exists further in the stream.
        /// </para>
        /// </remarks>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the section header (reader only, and only when sections are enabled).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The writer or reader has already been completed, or the writer/reader has already
        /// been completed via <see cref="IWriter.Complete"/> or <see cref="IReader.IsCompleted"/>.
        /// </exception>
        void BeginSection();

        /// <summary>
        /// Marks the end of the most recently opened section. When sections are enabled, this restores
        /// the previous stream boundary and (for readers) automatically advances past any remaining
        /// unread bytes in the section to maintain stream alignment.
        /// </summary>
        /// <returns>
        /// For readers, this method returns a boolean indicating whether the section was fully consumed
        /// (i.e., whether the reader's current position is exactly at the end of the section's recorded boundary).
        /// <see langword="true"/> if the section was fully consumed (no bytes were skipped), or if sections are disabled.
        /// <see langword="false"/> if the section contained unread bytes that were automatically skipped.
        /// For writers, this method returns
        /// <see langword="true"/> if sections are enabled and
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Must be preceded by a matching <see cref="BeginSection"/> call.
        /// Sections must be closed in reverse order of opening (LIFO).
        /// </para>
        /// <para>
        /// The BeginSection/EndSection nesting contract is <b>always enforced</b>, regardless
        /// of whether sections are enabled or disabled. Calling <see cref="EndSection"/> without
        /// a preceding <see cref="BeginSection"/> always throws <see cref="InvalidOperationException"/>.
        /// </para>
        /// <para>
        /// When sections are disabled, no boundary tracking occurs and the method always returns
        /// <see langword="true"/>, but the nesting contract above still applies.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Called without a matching open <see cref="BeginSection"/>, regardless of whether
        /// sections are enabled or disabled, or the writer/reader has already been completed.
        /// </exception>
        bool EndSection();
    }
}