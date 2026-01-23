namespace Archivarius.DataModels
{
    public struct BytesWrapper : IDataStruct
    {
        public byte[]? Value;

        public BytesWrapper(byte[]? value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}