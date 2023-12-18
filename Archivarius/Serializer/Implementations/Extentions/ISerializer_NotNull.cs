using System;

namespace Archivarius
{
    public static partial class ISerializer_Ext // NotNull
    {
        /// <summary>
        /// NotNull primitive class 
        /// </summary>
        public static void Add<T>(this IPrimitiveClassSerializer<T> serializer, ref T value, Func<T> defaultValue)
            where T : class
        {
            T? pValue = value;
            serializer.Add(ref pValue);
            value = pValue ?? defaultValue.Invoke();
        }
        
        /// <summary>
        /// NotNull DataStruct class
        /// </summary>
        public static void AddClass<T>(this ISerializer serializer, ref T value, Func<T> defaultValue)
            where T : class, IDataStruct
        {
            T? pValue = value;
            serializer.AddClass(ref pValue);
            value = pValue ?? defaultValue.Invoke();
        }
    }
}