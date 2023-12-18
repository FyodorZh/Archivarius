namespace Archivarius
{
    public class ULongSerializerExtension : ISerializerExtension<ulong>
    {
        public void Add(ISerializer serializer, ref ulong value)
        {
            serializer.Add(ref value);
        }
    }
}