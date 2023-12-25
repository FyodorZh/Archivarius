namespace Archivarius
{
    public class SByteSerializerExtension : ISerializerExtension<sbyte>
    {
        public void Add(ISerializer serializer, ref sbyte value)
        {
            serializer.Add(ref value);
        }
    }
}