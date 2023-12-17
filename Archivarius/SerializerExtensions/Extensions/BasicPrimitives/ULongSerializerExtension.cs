namespace Archivarius
{
    public class ULongSerializerExtension : ISerializerExtension<ulong>
    {
        public void Add(IOrderedSerializer serializer, ref ulong value)
        {
            serializer.Add(ref value);
        }
    }
}