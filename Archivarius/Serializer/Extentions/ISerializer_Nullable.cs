using System;
using System.Collections.Generic;

namespace Archivarius
{
    public static class ISerializer_Nullable
    {
        /// <summary>
        /// Nullable "primitive" value type 
        /// </summary>
        public static void Add<T>(this IPrimitiveSerializer<T> serializer, ref T? value)
            where T : struct
        {
            if (serializer.IsWriter)
            {
                bool isNull = value == null;
                serializer.Writer.WriteBool(isNull);
                if (!isNull)
                {
                    T tmp = value!.Value;
                    serializer.Add(ref tmp);
                }
            }
            else
            {
                bool isNull = serializer.Reader.ReadBool();
                if (isNull)
                {
                    value = null;
                }
                else
                {
                    T tmp = default(T);
                    serializer.Add(ref tmp);
                    value = tmp;
                }
            }
        }
        
        /// <summary>
        /// Nullable "DataStruct" value type 
        /// </summary>
        public static void Add<T>(this ISerializer serializer, ref T? value)
            where T : struct, IDataStruct
        {
            if (serializer.IsWriter)
            {
                bool isNull = value == null;
                serializer.Writer.WriteBool(isNull);
                if (!isNull)
                {
                    T tmp = value!.Value;
                    serializer.AddStruct(ref tmp);
                }
            }
            else
            {
                bool isNull = serializer.Reader.ReadBool();
                if (isNull)
                {
                    value = null;
                }
                else
                {
                    T tmp = default(T);
                    serializer.AddStruct(ref tmp);
                    value = tmp;
                }
            }
        }
    }
}