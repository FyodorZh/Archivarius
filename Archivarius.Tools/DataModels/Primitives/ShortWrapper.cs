namespace Archivarius.DataModels
{
    public struct ShortWrapper : IDataStruct
    {
        public short Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}