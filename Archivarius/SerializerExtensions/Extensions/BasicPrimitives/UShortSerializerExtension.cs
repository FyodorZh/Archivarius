namespace Archivarius
{
    public class UShortSerializerExtension : ISerializerExtension<ushort>
    {
        public void Add(IOrderedSerializer serializer, ref ushort value)
        {
            serializer.Add(ref value);
        }
    }
}