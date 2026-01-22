namespace Archivarius.DataModels
{
    public struct ByteWrapper : IDataStruct
    {
        public byte Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}