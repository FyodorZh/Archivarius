using System;

namespace Archivarius
{
    public static class ISerializer_NotNull
    {
        /// <summary>
        /// NotNull primitive class 
        /// </summary>
        public static void Add<T>(this IPrimitiveClassSerializer<T> serializer, ref T value, Func<T> defaultValue)
            where T : class
        {
            T? pValue = value;
            serializer.Add(ref pValue);
            if (!serializer.IsWriter)
            {
                value = pValue ?? defaultValue.Invoke();
            }
        }
        
        /// <summary>
        /// NotNull DataStruct class
        /// </summary>
        public static void AddClass<T>(this ISerializer serializer, ref T value, Func<T> defaultValue)
            where T : class, IDataStruct
        {
            T? pValue = value;
            serializer.AddClass(ref pValue);
            if (!serializer.IsWriter)
            {
                value = pValue ?? defaultValue.Invoke();
            }
        }
    }
}