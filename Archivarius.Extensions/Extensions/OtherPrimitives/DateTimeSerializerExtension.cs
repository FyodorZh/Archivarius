using System;

namespace Archivarius
{
    public class DateTimeSerializerExtension : ISerializerExtension<DateTime>
    {
        public void Add(ISerializer serializer, ref DateTime value)
        {
            if (serializer.IsWriter)
            {
                long v = value.ToBinary();
                serializer.Add(ref v);
            }
            else
            {
                long v = 0;
                serializer.Add(ref v);
                value = DateTime.FromBinary(v);
            }
        }
    }
}