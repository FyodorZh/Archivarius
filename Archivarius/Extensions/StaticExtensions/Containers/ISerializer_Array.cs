using System;

namespace Archivarius
{
    public static class ISerializer_Array
    {
        public static void AddArray<TValue>(
            this IPrimitiveSerializer<TValue> serializer,
            ref TValue[] array,
            Func<TValue[]> defaultValue)
        {
            var tmpArray = array;
            serializer.AddArray(ref tmpArray);
            if (!serializer.IsWriter)
            {
                array = tmpArray ?? defaultValue();
            }
        }
        
        public static void AddArray<TValue>(
            this IPrimitiveSerializer<TValue> serializer, 
            ref TValue[]? array)
        {
            if (serializer.IsWriter)
            {
                if (array != null)
                {
                    int count = array.Length;
                    serializer.Writer.WriteInt(count);
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref array[i]);
                    }
                }
                else
                {
                    serializer.Writer.WriteInt(-1);
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count < 0)
                {
                    array = null;
                }
                else if (count == 0)
                {
                    array = Array.Empty<TValue>();
                }
                else
                {
                    array = new TValue[count];

                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref array[i]);
                    }
                }
            }
        }
        
        public static void AddArray<TValue>(
            this IPrimitiveClassSerializer<TValue> serializer, 
            ref TValue[]? array,
            Func<TValue> defaultValue)
            where TValue : class
        {
            if (serializer.IsWriter)
            {
                if (array != null)
                {
                    int count = array.Length;
                    serializer.Writer.WriteInt(count);
                    for (int i = 0; i < count; ++i)
                    {
                        var tmp = array[i];
                        serializer.Add(ref tmp);
                    }
                }
                else
                {
                    serializer.Writer.WriteInt(-1);
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count < 0)
                {
                    array = null;
                }
                else if (count == 0)
                {
                    array = Array.Empty<TValue>();
                }
                else
                {
                    array = new TValue[count];

                    TValue? v = default(TValue);
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref v);
                        array[i] = v ?? defaultValue.Invoke();
                    }
                }
            }
        }
        
        public static void AddArray<TValue>(
            this ISerializer serializer, 
            ref TValue[]? array,
            ISerializer_AddMethod<TValue> addValue)
        {
            if (serializer.IsWriter)
            {
                if (array != null)
                {
                    int count = array.Length;
                    serializer.Writer.WriteInt(count);
                    for (int i = 0; i < count; ++i)
                    {
                        addValue.Invoke(serializer, ref array[i]);
                    }
                }
                else
                {
                    serializer.Writer.WriteInt(-1);
                }
            }
            else
            {
                int count = serializer.Reader.ReadInt();
                if (count < 0)
                {
                    array = null;
                }
                else if (count == 0)
                {
                    array = Array.Empty<TValue>();
                }
                else
                {
                    array = new TValue[count];
                    for (int i = 0; i < count; ++i)
                    {
                        addValue.Invoke(serializer, ref array[i]);
                    }
                }
            }
        }
    }
}