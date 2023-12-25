using System;
using System.Collections.Generic;

namespace Archivarius
{
    public static class ISerializer_List
    {
        #region Primitive value types
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
        #endregion

        #region Primitive class types
        public static void AddList<TValue>(
            this IPrimitiveClassSerializer<TValue> serializer,
            ref List<TValue> list,
            Func<List<TValue>> defaultValue,
            Func<TValue> defaultElementValue)
            where TValue : class
        {
            List<TValue>? tmpList = list;
            serializer.AddList(ref tmpList, defaultElementValue);
            if (!serializer.IsWriter)
            {
                list = tmpList ?? defaultValue();
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
        #endregion
        
        #region NonPrimitive types
        public static void AddList<TValue>(
            this ISerializer serializer, 
            ref List<TValue> list,
            Func<List<TValue>> defaultValue,
            ISerializer_AddMethod<TValue> addValue)
        {
            List<TValue>? tmpList = list;
            serializer.AddList(ref tmpList, addValue);
            if (!serializer.IsWriter)
            {
                list = tmpList ?? defaultValue();
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
        #endregion
    }
}