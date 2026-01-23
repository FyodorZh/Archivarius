namespace Archivarius.DataModels
{
    public struct UShortWrapper : IDataStruct
    {
        public ushort Value;

        public UShortWrapper(ushort value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}