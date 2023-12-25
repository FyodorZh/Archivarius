using System;

namespace Archivarius
{
    public sealed class SerializerExtensionsEmptyFactory : ISerializerExtensionsFactory
    {
        public static readonly ISerializerExtensionsFactory Instance = new SerializerExtensionsEmptyFactory();

        event Action<Type, Exception>? ISerializerExtensionsFactory.OnError
        {
            add {}
            remove {}
        }

        ISerializerExtension<T>? ISerializerExtensionsFactory.Construct<T>()
        {
            return null;
        }

        ISerializerExtension? ISerializerExtensionsFactory.Construct(Type type)
        {
            return null;
        }
    }
}