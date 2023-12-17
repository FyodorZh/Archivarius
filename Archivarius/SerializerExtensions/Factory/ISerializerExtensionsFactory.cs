using System;

namespace Archivarius
{
    public interface ISerializerExtensionsFactory
    {
        event Action<Type, Exception> OnError;
        ISerializerExtension<T>? Construct<T>();
        ISerializerExtension? Construct(Type type);
    }
}