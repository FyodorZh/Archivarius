namespace Archivarius
{
    public class ByteSerializerExtension : ISerializerExtension<byte>
    {
        public void Add(IOrderedSerializer serializer, ref byte value)
        {
            serializer.Add(ref value);
        }
    }
}