namespace Archivarius.DataModels
{
    public struct StringWrapper : IDataStruct
    {
        public string? Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}