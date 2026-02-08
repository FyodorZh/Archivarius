namespace Archivarius.Constructors
{
    public interface IConstructor
    {
        bool IsValid { get; }
        object? Construct();
    }
}