namespace Archivarius
{
    public interface IFlushCapability
    {
        /// <summary>
        /// Describes if implementation supports partial flush of collected data in the writer and using it in the reader.
        /// When <see langword="true"/>, the writer can flush <see cref="IWriter.Flush()"/> the collected data at specific times,
        /// and the reader can fetch it <see cref="IReader.Fetch()"/>.
        /// When <see langword="false"/>, the writer flushes all collected data by using <see cref="IWriter.Complete()"/>,
        /// and the reader fetches them by using <see cref="IReader.Fetch()"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IReader"/> and <see cref="IWriter"/> always expose the same value for
        /// this property — it is an implementation-defined constant.
        /// </remarks>
        bool FlushSupport { get; }
    }
}