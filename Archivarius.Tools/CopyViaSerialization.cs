using System;
using Archivarius.TypeSerializers;

namespace Archivarius
{
    internal static class CopyViaSerialization
    {
        [ThreadStatic] 
        private static bool _initialized;
        [ThreadStatic] 
        private static ReaderWriterStream _backend = null!;
        
        [ThreadStatic]
        private static HierarchicalSerializer _serializer = null!;
        
        [ThreadStatic]
        private static HierarchicalDeserializer _deserializer = null!;

        static void CheckSerializer()
        {
            if (!_initialized)
            {
                _initialized = true;
                _backend = new ReaderWriterStream();
                _serializer = new HierarchicalSerializer(_backend, new TypenameBasedTypeSerializer());
                _deserializer = HierarchicalDeserializer.From(_backend).SetPolymorphic(new TypenameBasedTypeDeserializer()).Build();
            }
        }

        public static T? CopyClass<T>(T? source) where T : class, IDataStruct
        {
            CheckSerializer();

            try
            {
                _serializer.Prepare();
                _serializer.AddClass(ref source);
                T? copy = default(T);
                _deserializer.Prepare();
                _deserializer.AddClass(ref copy);
                if (!_backend.IsEmpty)
                {
                    throw new InvalidOperationException();
                }
                return copy;
            }
            finally
            {
                _backend.Clear();   
            }
        }

        public static T CopyStruct<T>(T source) where T : struct, IDataStruct
        {
            CheckSerializer();
            
            try
            {
                _serializer.Prepare();
                _serializer.AddStruct(ref source);
                T copy = default(T);
                _deserializer.Prepare();
                _deserializer.AddStruct(ref copy);
                if (!_backend.IsEmpty)
                {
                    throw new InvalidOperationException();
                }
                return copy;
            }
            finally
            {
                _backend.Clear();   
            }
        }
    }


    public static class CopyViaSerialization_ClassExt
    {
        public static T? Copy<T>(this T? source) where T : class, IDataStruct
        {
            return CopyViaSerialization.CopyClass(source);
        }
    }
    public static class CopyViaSerialization_StructExt
    {   
        public static T Copy<T>(this T source) where T : struct, IDataStruct
        {
            return CopyViaSerialization.CopyStruct(source);
        }
    }
}