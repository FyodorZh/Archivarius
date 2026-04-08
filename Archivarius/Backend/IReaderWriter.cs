namespace Archivarius
{
    public interface IWriter : IWriterBackend, ILowLevelWriter
    {
    }

    public interface IReader : IReaderBackend, ILowLevelReader
    {
    }
}