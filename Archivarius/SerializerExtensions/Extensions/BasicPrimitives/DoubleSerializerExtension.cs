namespace Archivarius
{
    public class DoubleSerializerExtension : ISerializerExtension<double>
    {
        public void Add(IOrderedSerializer serializer, ref double value)
        {
            serializer.Add(ref value);
        }
    }
}