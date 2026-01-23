namespace Archivarius.DataModels
{
    public struct IntWrapper : IDataStruct
    {
        public int Value;

        public IntWrapper(int value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}