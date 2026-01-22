namespace Archivarius.DataModels
{
    public struct LongWrapper : IDataStruct
    {
        public long Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}