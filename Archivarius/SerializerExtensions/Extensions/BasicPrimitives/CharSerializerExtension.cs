namespace Archivarius
{
    public class CharSerializerExtension : ISerializerExtension<char>
    {
        public void Add(ISerializer serializer, ref char value)
        {
            serializer.Add(ref value);
        }
    }
}