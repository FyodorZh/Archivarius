namespace Archivarius
{
    public class BoolSerializerExtension : ISerializerExtension<bool>
    {
        public void Add(IOrderedSerializer serializer, ref bool value)
        {
            serializer.Add(ref value);
        }
    }
}