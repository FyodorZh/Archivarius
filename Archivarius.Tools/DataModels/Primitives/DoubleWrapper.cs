namespace Archivarius.DataModels
{
    public struct DoubleWrapper : IDataStruct
    {
        public double Value;

        public DoubleWrapper(double value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}