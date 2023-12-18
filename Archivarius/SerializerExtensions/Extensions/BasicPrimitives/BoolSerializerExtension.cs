namespace Archivarius
{
    public class BoolSerializerExtension : ISerializerExtension<bool>
    {
        public void Add(ISerializer serializer, ref bool value)
        {
            serializer.Add(ref value);
        }
    }
}