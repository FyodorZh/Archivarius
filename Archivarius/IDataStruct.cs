namespace Archivarius
{
    /// <summary>
    /// Data structure that can be serialized by <see cref="ISerializer"/>
    /// </summary>
    public interface IDataStruct
    {
        void Serialize(ISerializer serializer);
    }
}