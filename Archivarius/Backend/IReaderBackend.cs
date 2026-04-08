using System;
using System.Threading.Tasks;

namespace Archivarius
{
    /// <summary>
    /// Extends <see cref="ILowLevelReader"/> with section management and data fetching.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Whether type mismatches are detected depends on the implementation.
    /// Implementations that support type safety will throw <see cref="System.IO.InvalidDataException"/>
    /// whenever a read method is called for a type that differs from the type that was written
    /// (e.g. calling <see cref="ILowLevelReader.ReadInt"/> when a <see cref="ILowLevelWriter.WriteFloat"/> was written, or
    /// calling <see cref="ILowLevelReader.ReadBytes"/> when a typed value was written). Type safety is strict:
    /// signed/unsigned variants are distinct types (<c>short</c> ≠ <c>ushort</c>, etc.).
     /// Implementations that do not guarantee type safety must still detect data corruption in a
     /// weaker form: if sections are enabled they must report unconsumed or overconsumed bytes at
     /// each <see cref="ISectionsCapability.EndSection"/>; if sections are disabled they must detect insufficient data
     /// at the end of the stream.
    /// </para>
    /// </remarks>
    public interface IReaderBackend : IAnyBackend
    {
        ILowLevelReader Reader { get; }
        
        /// <summary>
        /// Indicates whether the reader has reached the permanent end of the data stream —
        /// no further data will ever become available.
        /// </summary>
        /// <remarks>
        /// This property becomes <see langword="true"/> only after <see cref="Fetch"/> has
        /// returned <see langword="false"/>. Before that point the value is
        /// <see langword="false"/> regardless of how much data remains in the current buffer.
        /// </remarks>
        bool IsCompleted { get; }

        /// <summary>
        /// Asynchronously loads the next logical block of data from the underlying source,
        /// making it available for reading.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if data is (or may be) available and deserialization can
        /// proceed; <see langword="false"/> if no more data will ever be available and
        /// deserialization must stop.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Always call <see cref="Fetch"/> before reading any data.</b>
        /// Attempting to read data without a preceding <see cref="Fetch"/> call is undefined
        /// behaviour — the data may simply not be present yet, and the outcome is
        /// implementation-specific.
        /// </para>
        /// <para>
        /// <b>Flush/Fetch must be matched 1-to-1.</b>
        /// There is no guarantee about how many bytes a single <see cref="Fetch"/> call will
        /// make available; that is an implementation detail of the underlying transport. The
        /// only reliable usage pattern is therefore for both sides to perform an equal number
        /// of <see cref="IWriterBackend.Flush"/>/<see cref="Fetch"/> calls in lockstep — one
        /// <see cref="IWriterBackend.Flush"/> on the writer for every <see cref="Fetch"/> on the
        /// reader, in the same order. Mismatching counts is not itself a hard error, but the
        /// resulting behaviour is undefined.
        /// </para>
        /// <para>
        /// <b><see cref="IWriterBackend.Complete"/> counts as one <see cref="IWriterBackend.Flush"/>.</b>
        /// A writer that writes data and then calls <see cref="IWriterBackend.Complete"/> (without a
        /// prior explicit <see cref="IWriterBackend.Flush"/>) makes that data available via exactly
        /// one <see cref="Fetch"/> call. The subsequent <see cref="Fetch"/> will return
        /// <see langword="false"/>.
        /// </para>
        /// <para>
        /// <b>Blocking behaviour.</b> For streaming implementations, if <see cref="Fetch"/> is
        /// called before the paired writer has called <see cref="IWriterBackend.Flush"/>, the call
        /// will suspend (await) until the data arrives.
        /// </para>
        /// <para>
        /// <b>No-data flushes.</b> <see cref="Fetch"/> may return <see langword="true"/> even
        /// when the paired <see cref="IWriterBackend.Flush"/> wrote no additional data. Callers must
        /// not assume that <see langword="true"/> means new bytes are present.
        /// </para>
        /// <para>
        /// For in-memory implementations this method completes synchronously and simply
        /// reports whether the buffer has unread bytes remaining.
        /// For streaming implementations it reads the next framed block from the transport.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The reader is in an invalid state, such as when a section is currently open.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while fetching data.</exception>
        ValueTask<bool> Fetch();
    }
}