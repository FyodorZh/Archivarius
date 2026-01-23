namespace Archivarius.DataModels
{
    public struct StructsArray<TDataStruct> : IDataStruct
        where TDataStruct : struct, IDataStruct
    {
        public TDataStruct[]? Value;

        public StructsArray(TDataStruct[]? value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.AddArray(ref Value, (ISerializer s, ref TDataStruct value) => s.AddStruct(ref value));
        }
    }
}