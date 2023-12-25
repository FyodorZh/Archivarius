namespace Archivarius
{
    public class ShortSerializerExtension : ISerializerExtension<short>
    {
        public void Add(ISerializer serializer, ref short value)
        {
            serializer.Add(ref value);
        }
    }
}