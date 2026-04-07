namespace Archivarius
{
    /// <summary>
    /// Describes the support level of a backend capability.
    /// </summary>
    public enum CapabilityState
    {
        /// <summary>
        /// The capability is not supported by the implementation.
        /// </summary>
        /// <remarks>
        /// Implementations that do not support a capability must still accept requests to disable the feature and return <c>true</c>,
        /// because disabling unsupported functionality is always valid operation. Requests to enable the
        /// feature must be rejected and return <c>false</c>.
        /// </remarks>
        NotSupported,

        /// <summary>
        /// The capability is supported and may be enabled or disabled by the caller.
        /// </summary>
        /// <remarks>
        /// The initial state is implementation-defined; callers should explicitly set the
        /// desired mode before performing any I/O.
        /// </remarks>
        Supported,

        /// <summary>
        /// The capability is mandatory for this implementation and cannot be disabled.
        /// </summary>
        /// <remarks>
        /// Attempts to enable the feature should succeed or
        /// simply report that the feature is already active (return <c>true</c>). Attempts
        /// to disable the feature must be rejected and return <c>false</c>.
        /// </remarks>
        Mandatory
    }
}