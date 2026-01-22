namespace Archivarius.DataModels
{
    public struct SByteWrapper : IDataStruct
    {
        public sbyte Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}