namespace Archivarius
{
    public class FloatSerializerExtension : ISerializerExtension<float>
    {
        public void Add(ISerializer serializer, ref float value)
        {
            serializer.Add(ref value);
        }
    }
}