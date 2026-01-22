namespace Archivarius.DataModels
{
    public struct FloatWrapper : IDataStruct
    {
        public float Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}