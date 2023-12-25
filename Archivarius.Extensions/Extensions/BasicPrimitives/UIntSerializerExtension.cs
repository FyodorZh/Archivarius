namespace Archivarius
{
    public class UIntSerializerExtension : ISerializerExtension<uint>
    {
        public void Add(ISerializer serializer, ref uint value)
        {
            serializer.Add(ref value);
        }
    }
}