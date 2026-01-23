namespace Archivarius.DataModels
{
    public struct ULongWrapper : IDataStruct
    {
        public ulong Value;

        public ULongWrapper(ulong value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}