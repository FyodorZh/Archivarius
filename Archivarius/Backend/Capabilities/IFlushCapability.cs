namespace Archivarius
{
    public interface IFlushCapability
    {
        /// <summary>
        /// Describes if implementation supports partial flush of collected data in the writer and using it in the reader.
        /// When <see langword="true"/>, the writer can flush <see cref="IWriterBackend.Flush()"/> the collected data at specific times,
        /// and the reader can fetch it <see cref="IReaderBackend.Fetch()"/>.
        /// When <see langword="false"/>, the writer flushes all collected data by using <see cref="IWriterBackend.Complete()"/>,
        /// and the reader fetches them by using <see cref="IReaderBackend.Fetch()"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IReaderBackend"/> and <see cref="IWriterBackend"/> always expose the same value for
        /// this property — it is an implementation-defined constant.
        /// </remarks>
        bool FlushSupport { get; }
    }
}