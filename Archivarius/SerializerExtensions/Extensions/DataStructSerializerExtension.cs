namespace Archivarius
{
    public class DataStructSerializerExtension<T> : ISerializerExtension<T>
        where T : struct, IDataStruct
    {
        public void Add(ISerializer serializer, ref T value)
        {
            serializer.AddStruct(ref value);
        }
    }
}