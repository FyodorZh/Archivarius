namespace Archivarius.DataModels
{
    public struct BoolWrapper : IDataStruct
    {
        public bool Value;

        public BoolWrapper(bool value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}