namespace Archivarius
{
    public interface ISerializerExtension
    {
    }

    public interface ISerializerExtension<T> : ISerializerExtension
    {
        void Add(IOrderedSerializer serializer, ref T value);
    }
}