namespace Archivarius.DataModels
{
    public struct DecimalWrapper : IDataStruct
    {
        public decimal Value;

        public DecimalWrapper(decimal value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}