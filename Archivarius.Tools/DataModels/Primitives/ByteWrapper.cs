namespace Archivarius.DataModels
{
    public struct ByteWrapper : IDataStruct
    {
        public byte Value;

        public ByteWrapper(byte value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}