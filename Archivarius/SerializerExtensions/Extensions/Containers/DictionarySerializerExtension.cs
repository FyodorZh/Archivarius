using System.Collections.Generic;

namespace Archivarius
{
    public class DictionarySerializerExtension<TKey, TValue> : ISerializerExtension<Dictionary<TKey, TValue>?>
        where TKey : notnull
    {
        private readonly ISerializerExtension<TKey> _keySerializer;
        private readonly ISerializerExtension<TValue> _valueSerializer;

        public DictionarySerializerExtension(ISerializerExtension<TKey> keySerializer, ISerializerExtension<TValue> valueSerializer)
        {
            _keySerializer = keySerializer;
            _valueSerializer = valueSerializer;
        }

        public void Add(ISerializer serializer, ref Dictionary<TKey, TValue>? value)
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
                    foreach (var kv in value)
                    {
                        var k = kv.Key;
                        var v = kv.Value;
                        _keySerializer.Add(serializer, ref k);
                        _valueSerializer.Add(serializer, ref v);
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
                    value = new Dictionary<TKey, TValue>(count);
                    for (int i = 0; i < count; ++i)
                    {
                        TKey k = default!;
                        TValue v = default!;
                        _keySerializer.Add(serializer, ref k);
                        _valueSerializer.Add(serializer, ref v);

                        value.Add(k, v);
                    }
                }
            }
        }
    }
}