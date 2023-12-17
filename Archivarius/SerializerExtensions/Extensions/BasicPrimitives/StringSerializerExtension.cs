namespace Archivarius
{
    public class StringSerializerExtension : ISerializerExtension<string?>
    {
        public void Add(IOrderedSerializer serializer, ref string? value)
        {
            serializer.Add(ref value);
        }
    }
}