namespace Archivarius.Constructors
{
    public class GenericTypeConstructor<T> : IConstructor
        where T : class, new()
    {
        public bool IsValid => true;

        public object Construct()
        {
            return new T();
        }
    }
}