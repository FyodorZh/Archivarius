namespace Archivarius
{
    public class SByteSerializerExtension : ISerializerExtension<sbyte>
    {
        public void Add(IOrderedSerializer serializer, ref sbyte value)
        {
            serializer.Add(ref value);
        }
    }
}