namespace Archivarius.DataModels
{
    public struct ULongWrapper : IDataStruct
    {
        public ulong Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}