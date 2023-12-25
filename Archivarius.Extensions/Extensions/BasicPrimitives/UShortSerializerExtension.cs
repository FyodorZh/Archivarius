namespace Archivarius
{
    public class UShortSerializerExtension : ISerializerExtension<ushort>
    {
        public void Add(ISerializer serializer, ref ushort value)
        {
            serializer.Add(ref value);
        }
    }
}