namespace Archivarius
{
    public class IntSerializerExtension : ISerializerExtension<int>
    {
        public void Add(ISerializer serializer, ref int value)
        {
            serializer.Add(ref value);
        }
    }
}