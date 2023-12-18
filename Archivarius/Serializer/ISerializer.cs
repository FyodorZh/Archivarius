namespace Archivarius
{
    public interface ISerializerCore
    {
        bool IsWriter { get; }
        ILowLevelReader Reader { get; }
        ILowLevelWriter Writer { get; }
    }

    public interface IPrimitiveSerializer<T> : ISerializerCore
    {
        void Add(ref T value);
    }

    public interface IPrimitiveClassSerializer<T> : IPrimitiveSerializer<T?>
        where T : class
    {
    }
        
    public interface IPrimitiveSerializer :
        IPrimitiveSerializer<bool>,
        IPrimitiveSerializer<byte>,
        IPrimitiveSerializer<sbyte>,
        IPrimitiveSerializer<char>,
        IPrimitiveSerializer<short>,
        IPrimitiveSerializer<ushort>,
        IPrimitiveSerializer<int>,
        IPrimitiveSerializer<uint>,
        IPrimitiveSerializer<long>,
        IPrimitiveSerializer<ulong>,
        IPrimitiveSerializer<float>,
        IPrimitiveSerializer<double>,
        IPrimitiveClassSerializer<string>,
        IPrimitiveClassSerializer<byte[]>
    {
    }

    public interface ISerializer : IPrimitiveSerializer
    {
        byte Version { get; }

        void AddStruct<T>(ref T value) where T : struct, IDataStruct;
        void AddVersionedStruct<T>(ref T value) where T : struct, IDataStruct, IVersionedData;

        /// <summary>
        /// Serialize both versioned and unversioned data
        /// </summary>
        void AddClass<T>(ref T? value) where T : class, IDataStruct;

        void AddAny<T>(ref T value);
    }
}