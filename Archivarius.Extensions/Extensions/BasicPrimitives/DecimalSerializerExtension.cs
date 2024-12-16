namespace Archivarius
{
    public class DecimalSerializerExtension : ISerializerExtension<decimal>
    {
        public void Add(ISerializer serializer, ref decimal value)
        {
            serializer.Add(ref value);
        }
    }
}