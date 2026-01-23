namespace Archivarius.DataModels
{
    public struct SByteWrapper : IDataStruct
    {
        public sbyte Value;

        public SByteWrapper(sbyte value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}