namespace Archivarius
{
    public class ByteSerializerExtension : ISerializerExtension<byte>
    {
        public void Add(ISerializer serializer, ref byte value)
        {
            serializer.Add(ref value);
        }
    }
}