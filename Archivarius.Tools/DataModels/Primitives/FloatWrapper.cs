namespace Archivarius.DataModels
{
    public struct FloatWrapper : IDataStruct
    {
        public float Value;

        public FloatWrapper(float value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}