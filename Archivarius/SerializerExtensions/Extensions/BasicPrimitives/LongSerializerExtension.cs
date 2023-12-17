namespace Archivarius
{
    public class LongSerializerExtension : ISerializerExtension<long>
    {
        public void Add(IOrderedSerializer serializer, ref long value)
        {
            serializer.Add(ref value);
        }
    }
}