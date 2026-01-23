namespace Archivarius.DataModels
{
    public struct LongWrapper : IDataStruct
    {
        public long Value;

        public LongWrapper(long value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}