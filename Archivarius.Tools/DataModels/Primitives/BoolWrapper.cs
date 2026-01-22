namespace Archivarius.DataModels
{
    public struct BoolWrapper : IDataStruct
    {
        public bool Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}