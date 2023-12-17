using System.Collections.Generic;

namespace Archivarius
{
    public class ListSerializerExtension<T> : ISerializerExtension<List<T>?>
    {
        private readonly ISerializerExtension<T> _elementSerializer;

        public ListSerializerExtension(ISerializerExtension<T> elementSerializer)
        {
            _elementSerializer = elementSerializer;
        }

        public void Add(IOrderedSerializer serializer, ref List<T>? value)
        {
            if (serializer.IsWriter)
            {
                if (value == null)
                {
                    serializer.Writer.WriteInt(0);
                }
                else
                {
                    serializer.Writer.WriteInt(value.Count + 1);
                    foreach (var element in value)
                    {
                        var tmp = element;
                        _elementSerializer.Add(serializer, ref tmp);
                    }
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count == 0)
                {
                    value = null;
                }
                else
                {
                    count -= 1;
                    value = new List<T>(count);
                    for (int i = 0; i < count; ++i)
                    {
                        T element = default!;
                        _elementSerializer.Add(serializer, ref element);
                        value.Add(element);
                    }
                }
            }
        }
    }
}