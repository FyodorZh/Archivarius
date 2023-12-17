namespace Archivarius
{
    public class IntSerializerExtension : ISerializerExtension<int>
    {
        public void Add(IOrderedSerializer serializer, ref int value)
        {
            serializer.Add(ref value);
        }
    }
}