using System;

namespace Archivarius
{
    public class DateTimeSerializerExtension : ISerializerExtension<DateTime>
    {
        public void Add(ISerializer serializer, ref DateTime value)
        {
            serializer.Add(ref value);
        }
    }
}