namespace Archivarius.DataModels
{
    public struct UIntWrapper : IDataStruct
    {
        public uint Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}