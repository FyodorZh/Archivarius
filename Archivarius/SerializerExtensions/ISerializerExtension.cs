namespace Archivarius
{
    public interface ISerializerExtension
    {
    }

    public interface ISerializerExtension<T> : ISerializerExtension
    {
        void Add(ISerializer serializer, ref T value);
    }
}