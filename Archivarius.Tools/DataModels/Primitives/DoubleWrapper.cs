namespace Archivarius.DataModels
{
    public struct DoubleWrapper : IDataStruct
    {
        public double Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}