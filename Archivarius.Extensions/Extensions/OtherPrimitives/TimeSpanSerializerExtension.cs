using System;

namespace Archivarius
{
    public class TimeSpanSerializerExtension : ISerializerExtension<TimeSpan>
    {
        public void Add(ISerializer serializer, ref TimeSpan value)
        {
            serializer.Add(ref value);
        }
    }
}