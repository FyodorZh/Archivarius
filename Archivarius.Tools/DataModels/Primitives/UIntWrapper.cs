namespace Archivarius.DataModels
{
    public struct UIntWrapper : IDataStruct
    {
        public uint Value;

        public UIntWrapper(uint value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}