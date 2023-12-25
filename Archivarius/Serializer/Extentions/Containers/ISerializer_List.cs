using System;
using System.Collections.Generic;

namespace Archivarius
{
    public static class ISerializer_List
    {
        public static void AddList<TValue>(
            this IPrimitiveSerializer<TValue> serializer,
            ref List<TValue> array,
            Func<List<TValue>> defaultValue)
        {
            var tmpList = array;
            serializer.AddList(ref tmpList);
            if (!serializer.IsWriter)
            {
                array = tmpList ?? defaultValue();
            }
        }
        
        public static void AddList<TValue>(
            this IPrimitiveSerializer<TValue> serializer, 
            ref List<TValue>? list)
        {
            if (serializer.IsWriter)
            {
                if (list != null)
                {
                    serializer.Writer.WriteInt(list.Count);
                    foreach (TValue element in list)
                    {
                        TValue tmpElement = element;
                        serializer.Add(ref tmpElement);
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
                    list = null;
                }
                else
                {
                    list = new List<TValue>(count);

                    TValue element = default(TValue)!;
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref element);
                        list.Add(element);
                    }
                }
            }
        }
        
        public static void AddList<TValue>(
            this IPrimitiveClassSerializer<TValue> serializer, 
            ref List<TValue>? list,
            Func<TValue> defaultValue)
            where TValue : class
        {
            if (serializer.IsWriter)
            {
                if (list != null)
                {
                    serializer.Writer.WriteInt(list.Count);
                    foreach (TValue? element in list)
                    {
                        TValue? tmpElement = element;
                        serializer.Add(ref tmpElement);
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
                    list = null;
                }
                else
                {
                    list = new List<TValue>(count);

                    TValue? element = default(TValue);
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Add(ref element);
                        list.Add(element ?? defaultValue());
                    }
                }
            }
        }
        
        public static void AddList<TValue>(
            this ISerializer serializer, 
            ref List<TValue>? list,
            ISerializer_AddMethod<TValue> addValue)
        {
            if (serializer.IsWriter)
            {
                if (list != null)
                {
                    serializer.Writer.WriteInt(list.Count);
                    foreach (TValue element in list)
                    {
                        TValue tmpElement = element;
                        addValue.Invoke(serializer, ref tmpElement);
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
                    list = null;
                }
                else
                {
                    list = new List<TValue>(count);

                    TValue element = default(TValue)!;
                    for (int i = 0; i < count; ++i)
                    {
                        addValue.Invoke(serializer, ref element);
                        list.Add(element);
                    }
                }
            }
        }
    }
}