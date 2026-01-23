namespace Archivarius.DataModels
{
    public struct CharWrapper : IDataStruct
    {
        public char Value;

        public CharWrapper(char value)
        {
            Value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref Value);
        }
    }
}