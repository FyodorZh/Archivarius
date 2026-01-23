namespace Archivarius.DataModels
{
    public struct Pair<TFirst, TSecond> : IDataStruct
        where TFirst : struct, IDataStruct
        where TSecond : struct, IDataStruct
    {
        public TFirst First;
        public TSecond Second;

        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.AddStruct(ref First);
            serializer.AddStruct(ref Second);
        }
    }
}