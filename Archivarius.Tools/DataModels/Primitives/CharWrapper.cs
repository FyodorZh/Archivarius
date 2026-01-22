namespace Archivarius.DataModels
{
    public struct CharWrapper : IDataStruct
    {
        public char Value;

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}