namespace Archivarius
{
    public class VersionedDataStructSerializerExtension<T> : ISerializerExtension<T>
        where T : struct, IVersionedDataStruct
    {
        public void Add(ISerializer serializer, ref T value)
        {
            serializer.AddVersionedStruct(ref value);
        }
    }
}