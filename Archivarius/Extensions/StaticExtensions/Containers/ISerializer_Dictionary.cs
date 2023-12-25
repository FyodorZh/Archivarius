using System;
using System.Collections.Generic;

namespace Archivarius
{
    public static class ISerializer_Dictionary
    {
        public static void AddDictionary<TKey, TValue, TSerializer>(
            this TSerializer serializer,
            ref Dictionary<TKey, TValue> dictionary,
            Func<Dictionary<TKey, TValue>> defaultValue)
            where TKey : notnull
            where TSerializer : IPrimitiveSerializer<TKey>, IPrimitiveSerializer<TValue>
        {
            var tmp = dictionary;
            serializer.AddDictionary(ref tmp);
            if (!serializer.IsWriter)
            {
                dictionary = tmp ?? defaultValue();
            }
        }

        public static void AddDictionary<TKey, TValue, TSerializer>(
            this TSerializer serializer, 
            ref Dictionary<TKey, TValue>? dictionary)
            where TKey : notnull
            where TSerializer : IPrimitiveSerializer<TKey>, IPrimitiveSerializer<TValue>
        {
            if (serializer.IsWriter)
            {
                if (dictionary == null)
                {
                    serializer.Writer.WriteInt(-1);
                }
                else
                {
                    serializer.Writer.WriteInt(dictionary.Count);
                    foreach (var kv in dictionary)
                    {
                        var k = kv.Key;
                        var v = kv.Value;
                        serializer.Add(ref k);
                        serializer.Add(ref v);
                    }
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count < 0)
                {
                    dictionary = null;
                }
                else
                {
                    dictionary = new Dictionary<TKey, TValue>(count);
                    
                    TKey k = default(TKey)!;
                    TValue v = default(TValue)!;
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref k);
                        serializer.Add(ref v);
                        dictionary.Add(k, v);
                    }
                }
            }
        }
        
        public static void AddDictionary<TKey, TValue>(
            this ISerializer serializer, 
            ref Dictionary<TKey, TValue> dictionary,
            Func<Dictionary<TKey, TValue>> defaultValue,
            ISerializer_AddMethod<TKey> addKey,
            ISerializer_AddMethod<TValue> addValue)
            where TKey : notnull
        {
            var tmp = dictionary;
            serializer.AddDictionary(ref tmp, addKey, addValue);
            if (!serializer.IsWriter)
            {
                dictionary = tmp ?? defaultValue();
            }
        }

        public static void AddDictionary<TKey, TValue>(
            this ISerializer serializer, 
            ref Dictionary<TKey, TValue>? dictionary,
            ISerializer_AddMethod<TKey> addKey,
            ISerializer_AddMethod<TValue> addValue)
            where TKey : notnull
        {
            if (serializer.IsWriter)
            {
                if (dictionary == null)
                {
                    serializer.Writer.WriteInt(-1);
                }
                else
                {
                    serializer.Writer.WriteInt(dictionary.Count);
                    foreach (var kv in dictionary)
                    {
                        var k = kv.Key;
                        var v = kv.Value;
                        addKey(serializer, ref k);
                        addValue(serializer, ref v);
                    }
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count < 0)
                {
                    dictionary = null;
                }
                else
                {
                    dictionary = new Dictionary<TKey, TValue>(count);
                    
                    TKey k = default(TKey)!;
                    TValue v = default(TValue)!;
                    for (int i = 0; i < count; ++i)
                    {
                        addKey(serializer, ref k);
                        addValue(serializer, ref v);
                        dictionary.Add(k, v);
                    }
                }
            }
        }
    }
}