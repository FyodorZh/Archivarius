namespace Archivarius
{
    /// <summary>
    /// Data that can be versioned by <see cref="ISerializer"/>.
    /// Versioning is used to provide backward compatibility when deserializing data with different versions
    /// of the serializer.
    /// </summary>
    public interface IVersionedData
    {
        byte Version { get; }
    }
}