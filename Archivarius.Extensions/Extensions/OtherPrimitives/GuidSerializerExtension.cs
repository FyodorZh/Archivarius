using System;

namespace Archivarius
{
    public class GuidSerializerExtension : ISerializerExtension<Guid>
    {
        public void Add(ISerializer serializer, ref Guid value)
        {
            serializer.Add(ref value);
        }
    }
}