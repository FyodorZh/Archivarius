using System;
using System.Collections.Generic;

namespace Archivarius
{
    public static class ISerializer_Containers
    {
        /// <summary>
        /// IReadOnlyList of primitive type 
        /// </summary>
        public static void Add<T>(this IPrimitiveSerializer<T?> serializer, ref IReadOnlyList<T?>? list)
        {
            if (serializer.IsWriter)
            {
                WriteList(serializer, list);
            }
            else
            {
                list = ReadAsArray(serializer);
            }
        }
        
        private static void WriteList<T>(IPrimitiveSerializer<T?> serializer, IReadOnlyList<T?>? list)
        {
            if (list == null)
            {
                serializer.Writer.WriteInt(0);
            }
            else
            {
                int count = list.Count;
                serializer.Writer.WriteInt(count + 1);
                for (int i = 0; i < count; ++i)
                {
                    var tmp = list[i];
                    serializer.Add(ref tmp);
                }
            }
        }

        private static T?[]? ReadAsArray<T>(IPrimitiveSerializer<T?> serializer)
        {
            int count = serializer.Reader.ReadInt();
            if (count <= 0)
            {
                return null;
            }

            count -= 1;

            if (count == 0)
            {
                return Array.Empty<T?>();
            }
            
            var array = new T?[count];
            for (int i = 0; i < count; ++i)
            {
                serializer.Add(ref array[i]);
            }

            return array;
        }
        
        private static List<T?>? ReadAsList<T>(IPrimitiveSerializer<T?> serializer)
        {
            int count = serializer.Reader.ReadInt();
            if (count <= 0)
            {
                return null;
            }

            count -= 1;
            
            var list = new List<T?>(count);
            for (int i = 0; i < count; ++i)
            {
                var element = default(T);
                serializer.Add(ref element);
                list.Add(element);
            }

            return list;
        }
    }
}