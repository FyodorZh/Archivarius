namespace Archivarius
{
    public class DoubleSerializerExtension : ISerializerExtension<double>
    {
        public void Add(ISerializer serializer, ref double value)
        {
            serializer.Add(ref value);
        }
    }
}