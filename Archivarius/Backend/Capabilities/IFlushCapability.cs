namespace Archivarius
{
    public interface IFlushCapability
    {
        CapabilityState FlushSupport { get; }
    }
}