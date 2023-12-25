using System;

namespace Archivarius
{
    public static class ISerializer_Array
    {
        public static void Add<TValue>(
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
        
        public static void Add<TValue>(
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