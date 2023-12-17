namespace Archivarius
{
    public class FloatSerializerExtension : ISerializerExtension<float>
    {
        public void Add(IOrderedSerializer serializer, ref float value)
        {
            serializer.Add(ref value);
        }
    }
}