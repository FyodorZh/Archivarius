namespace Archivarius
{
    public class DataClassSerializerExtension<T> : ISerializerExtension<T?>
        where T : class, IDataStruct
    {
        public void Add(IOrderedSerializer serializer, ref T? value)
        {
            serializer.AddClass(ref value);
        }
    }
}