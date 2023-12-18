namespace Archivarius
{
    public class ArraySerializerExtension<T> : ISerializerExtension<T[]?>
    {
        private readonly ISerializerExtension<T> _elementSerializer;

        public ArraySerializerExtension(ISerializerExtension<T> elementSerializer)
        {
            _elementSerializer = elementSerializer;
        }

        public void Add(ISerializer serializer, ref T[]? value)
        {
            if (serializer.IsWriter)
            {
                if (value == null)
                {
                    serializer.Writer.WriteInt(0);
                }
                else
                {
                    int count = value.Length;
                    serializer.Writer.WriteInt(count + 1);
                    for (int i = 0; i < count; ++i)
                    {
                        _elementSerializer.Add(serializer, ref value[i]);
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
                    value = new T[count];
                    for (int i = 0; i < count; ++i)
                    {
                        _elementSerializer.Add(serializer, ref value[i]);
                    }
                }
            }
        }
    }
}