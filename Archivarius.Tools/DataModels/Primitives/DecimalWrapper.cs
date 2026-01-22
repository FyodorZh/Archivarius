namespace Archivarius.DataModels
{
    public struct DecimalWrapper : IDataStruct
    {
        public decimal Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}