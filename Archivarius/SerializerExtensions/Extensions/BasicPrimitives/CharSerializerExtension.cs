namespace Archivarius
{
    public class CharSerializerExtension : ISerializerExtension<char>
    {
        public void Add(IOrderedSerializer serializer, ref char value)
        {
            serializer.Add(ref value);
        }
    }
}