namespace Archivarius.DataModels
{
    public struct UShortWrapper : IDataStruct
    {
        public ushort Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}