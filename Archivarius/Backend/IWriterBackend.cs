using System;
using System.Threading.Tasks;

namespace Archivarius
{
    public interface IWriterBackend : IAnyBackend
    {
        ILowLevelWriter Writer { get; }
        
        /// <summary>
        /// Commits all buffered data to the underlying store and signals that a complete
        /// logical block is ready to be consumed by the paired <see cref="IReaderBackend"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Flush/Fetch must be matched 1-to-1.</b>
        /// After a <see cref="IReaderBackend.Fetch"/> call the reader does not guarantee how many bytes
        /// are actually made available — this is an implementation detail of the underlying
        /// transport. Therefore the only reliable usage pattern is for the writer side and the
        /// reader side to perform an equal number of <see cref="Flush"/>/<see cref="IReaderBackend.Fetch"/>
        /// calls in lockstep. Mismatching counts is not itself a hard error, but the resulting
        /// behaviour is undefined.
        /// </para>
        /// <para>
        /// For in-memory implementations this method is a no-op and completes synchronously.
        /// For streaming implementations it frames and transmits the accumulated buffer to the
        /// underlying transport.
        /// </para>
        /// <para>
        /// When sections are <b>enabled</b>, this method must not be called while a section is
        /// open (i.e. after a <see cref="BeginSection"/> that has not yet been matched by
        /// <see cref="EndSection"/>). Doing so will throw <see cref="InvalidOperationException"/>.
        /// When sections are <b>disabled</b>, an unmatched <see cref="BeginSection"/> does
        /// <b>not</b> prevent flushing — the call succeeds normally.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Sections are enabled and a section is currently open, or the writer has already been
        /// completed via <see cref="Complete"/>.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while flushing data.</exception>
        ValueTask Flush();

        /// <summary>
        /// Flushes all buffered data and signals that no more data will be written.
        /// After this call returns, all subsequent calls to any method on this writer will
        /// throw <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All sections must be closed before calling this method.
        /// </para>
        /// <para>
        /// <b><see cref="Complete"/> counts as one <see cref="Flush"/> for lockstep purposes.</b>
        /// Data written before calling <see cref="Complete"/> (without a preceding explicit
        /// <see cref="Flush"/>) is made available to the paired reader via exactly one
        /// <see cref="IReaderBackend.Fetch"/> call. The reader's subsequent <see cref="IReaderBackend.Fetch"/>
        /// will return <see langword="false"/>.
        /// </para>
        /// <para>
        /// Calling this method more than once will throw <see cref="InvalidOperationException"/>
        /// on the second and all subsequent invocations.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// A section is currently open, or the writer has already been completed by a prior
        /// call to this method.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while flushing data.</exception>
        ValueTask Complete();
    }
}