namespace Archivarius
{
    public interface ITypeSafetyCapability
    {
        /// <summary>
        /// Describes how this implementation handles type-safety of data.
        /// </summary>
        /// <value>
        /// <list type="bullet">
        ///   <item>
        ///     <term><see cref="CapabilityState.NotSupported"/></term>
        ///     <description>
        ///       Type-safety is not supported for this implementation.
        ///       The only type of errors that can be detected is data size mismatch.
        ///       Type mismatches are not detected and may lead to undefined behavior.
        ///       <see cref="TrySetTypeSafety"/> always returns
        ///       <see langword="true"/> for <c>false</c> (disable) and
        ///       <see langword="false"/> for <c>true</c> (enable).
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CapabilityState.Supported"/></term>
        ///     <description>
        ///       Type-safety can be explicitly enabled or disabled via
        ///       <see cref="TrySetTypeSafety"/> before any data is written or read.
        ///       The type-safety state is not defined — callers must always set
        ///       the desired mode explicitly before any I/O.
        ///       Both <c>true</c> and <c>false</c> arguments are accepted and return
        ///       <see langword="true"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CapabilityState.Mandatory"/></term>
        ///     <description>
        ///       Type-safety permanently enabled; it cannot be turned off.
        ///       <see cref="TrySetTypeSafety"/> returns <see langword="true"/>
        ///       for <c>true</c> (already enabled) and <see langword="false"/> for
        ///       <c>false</c> (disabling is rejected).
        ///     </description>
        ///   </item>
        /// </list>
        /// </value>
        /// <remarks>
        /// <see cref="IReader"/> and <see cref="IWriter"/> always expose the same value for
        /// this property — it is an implementation-defined constant.
        /// </remarks>
        CapabilityState TypeSafetySupport { get; }
        
        /// <summary>
        /// Attempts to enable or disable type-safety.
        /// When type-safety is enabled, every <see cref="IWriter"/> implementation writes additional
        /// type info so <see cref="IWriter"/> can detect type mismatches when reading.
        /// When a mismatch is detected, the reader must throw an exception.
        /// When type-safety is disabled, no type info is written, and type mismatches are not detected, which may lead to undefined behavior.
        /// </summary>
        /// <param name="enabled">
        /// <see langword="true"/> to enable type-safety; <see langword="false"/> to disable it.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the mode was successfully applied.
        /// <see langword="false"/> if the request was rejected — either because data has already
        /// been read or written, or because the implementation does not support the requested mode.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Must be called before any data is read or written to the stream.
        /// The paired <see cref="IReader"/> and <see cref="IWriter"/> must be configured with the same value.
        /// </para>
        /// <para>
        /// An implementation that does not support type-safety must still accept
        /// <c>TrySetTypeSafety(false)</c> and return <see langword="true"/>, because
        /// disabling sections is always valid. It must return <see langword="false"/> only
        /// when <paramref name="enabled"/> is <see langword="true"/>.
        /// </para>
        /// </remarks>
        bool TrySetTypeSafety(bool enabled);
    }
}