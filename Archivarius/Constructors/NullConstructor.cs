namespace Archivarius.Constructors
{
    public class NullConstructor : IConstructor
    {
        public static readonly IConstructor Instance = new NullConstructor();

        public bool IsValid => false;

        public object? Construct()
        {
            return null;
        }
    }
}