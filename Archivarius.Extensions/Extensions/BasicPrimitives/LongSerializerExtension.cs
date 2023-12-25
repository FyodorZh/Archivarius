namespace Archivarius
{
    public class LongSerializerExtension : ISerializerExtension<long>
    {
        public void Add(ISerializer serializer, ref long value)
        {
            serializer.Add(ref value);
        }
    }
}