namespace Archivarius
{
    public class UIntSerializerExtension : ISerializerExtension<uint>
    {
        public void Add(IOrderedSerializer serializer, ref uint value)
        {
            serializer.Add(ref value);
        }
    }
}