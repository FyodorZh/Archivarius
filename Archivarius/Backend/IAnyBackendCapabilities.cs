namespace Archivarius
{
    public interface IAnyBackendCapabilities
    {
        enum CapabilityState
        {
            NotSupported,
            Supported,
            Mandatory
        }

        CapabilityState SectionsSupport { get; }
        CapabilityState TypeSafetySupport { get; }
        CapabilityState PartialFlushSupport { get; }
    }
}