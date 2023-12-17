namespace Archivarius
{
    public class ShortSerializerExtension : ISerializerExtension<short>
    {
        public void Add(IOrderedSerializer serializer, ref short value)
        {
            serializer.Add(ref value);
        }
    }
}