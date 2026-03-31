using System;
using System.Threading.Tasks;

namespace Archivarius
{
    /// <summary>
    /// Defines the low-level primitive write operations.
    /// Each method has a corresponding read method in <see cref="ILowLevelReader"/>
    /// that must be called in the same order to correctly deserialize the data.
    /// </summary>
    public interface ILowLevelWriter
    {
        /// <summary>Writes a <see cref="bool"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadBool"/>
        void WriteBool(bool value);

        /// <summary>Writes a <see cref="byte"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadByte"/>
        void WriteByte(byte value);

        /// <summary>Writes an <see cref="sbyte"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadSByte"/>
        void WriteSByte(sbyte value);

        /// <summary>Writes a <see cref="char"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadChar"/>
        void WriteChar(char value);

        /// <summary>Writes a <see cref="short"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadShort"/>
        void WriteShort(short value);

        /// <summary>Writes a <see cref="ushort"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadUShort"/>
        void WriteUShort(ushort value);

        /// <summary>Writes an <see cref="int"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadInt"/>
        void WriteInt(int value);

        /// <summary>Writes a <see cref="uint"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadUInt"/>
        void WriteUInt(uint value);

        /// <summary>Writes a <see cref="long"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadLong"/>
        void WriteLong(long value);

        /// <summary>Writes a <see cref="ulong"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadULong"/>
        void WriteULong(ulong value);

        /// <summary>Writes a <see cref="float"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadFloat"/>
        void WriteFloat(float value);

        /// <summary>Writes a <see cref="double"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadDouble"/>
        void WriteDouble(double value);

        /// <summary>Writes a <see cref="decimal"/> value.</summary>
        /// <seealso cref="ILowLevelReader.ReadDecimal"/>
        void WriteDecimal(decimal value);

        /// <summary>
        /// Writes a nullable UTF-8 encoded <see cref="string"/> value.
        /// A <see langword="null"/> value is encoded as a distinct sentinel so that
        /// <see cref="ILowLevelReader.ReadString"/> can faithfully return <see langword="null"/>.
        /// </summary>
        /// <seealso cref="ILowLevelReader.ReadString"/>
        void WriteString(string? value);

        /// <summary>
        /// Writes a nullable byte array.
        /// A <see langword="null"/> value is encoded as a distinct sentinel so that
        /// <see cref="ILowLevelReader.ReadArray"/> can faithfully return <see langword="null"/>.
        /// </summary>
        /// <seealso cref="ILowLevelReader.ReadArray"/>
        void WriteArray(byte[]? value);

        /// <summary>
        /// Writes <paramref name="count"/> raw bytes from <paramref name="value"/> starting
        /// at <paramref name="offset"/>. Must be paired with a corresponding
        /// <see cref="ILowLevelReader.ReadBytes"/> call that specifies the same
        /// <paramref name="count"/>.
        /// </summary>
        /// <param name="value">The source buffer.</param>
        /// <param name="offset">The zero-based start index within <paramref name="value"/>.</param>
        /// <param name="count">The number of bytes to write. When zero, the method is a no-op (nothing is written to the stream).</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="offset"/> + <paramref name="count"/> exceeds the length of <paramref name="value"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadBytes"/>
        void WriteBytes(byte[] value, int offset, int count);
    }

    /// <summary>
    /// Extends <see cref="ILowLevelWriter"/> with section management and flushing.
    /// </summary>
    public interface IWriter : ILowLevelWriter
    {
        /// <summary>
        /// Attempts to enable or disable logical section markers in the output stream.
        /// When sections are enabled, every <see cref="BeginSection"/>/<see cref="EndSection"/>
        /// pair embeds a byte-length header that allows the paired <see cref="IReader"/> to
        /// detect and skip unknown or added fields, providing forward and backward compatibility.
        /// </summary>
        /// <param name="useSections">
        /// <see langword="true"/> to write section length headers; <see langword="false"/> to omit them.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the mode was successfully applied.
        /// <see langword="false"/> if the request was rejected — either because data has already
        /// been written, or because the implementation does not support the requested mode.
        /// </returns>
        /// <remarks>
        /// Must be called before any data is written to the stream.
        /// The paired <see cref="IReader"/> must be configured with the same value.
        /// </remarks>
        bool TrySetSectionUsage(bool useSections);

        /// <summary>
        /// Marks the beginning of a logical section.
        /// When sections are enabled, reserves space in the stream for the section's
        /// byte-length header, which is filled in by the matching <see cref="EndSection"/> call.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Every <see cref="BeginSection"/> call must be balanced by exactly one
        /// <see cref="EndSection"/> call. Sections must be properly nested — i.e. sections
        /// behave like matching brackets: the most recently opened section must be closed first.
        /// </para>
        /// <para>
        /// Can be called regardless of whether sections are enabled via
        /// <see cref="TrySetSectionUsage"/>. When sections are disabled this method is a no-op,
        /// but the nesting contract must still be respected.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The writer is in an invalid state.</exception>
        void BeginSection();

        /// <summary>
        /// Marks the end of the most recently opened section and writes its byte-length
        /// header into the stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Must be preceded by a matching <see cref="BeginSection"/> call.
        /// Sections must be closed in reverse order of opening (LIFO).
        /// </para>
        /// <para>
        /// Can be called regardless of whether sections are enabled via
        /// <see cref="TrySetSectionUsage"/>. When sections are disabled this method is a no-op,
        /// but the nesting contract must still be respected.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Called without a matching open <see cref="BeginSection"/>.
        /// </exception>
        void EndSection();

        /// <summary>
        /// Commits all buffered data to the underlying store and signals that a complete
        /// logical block is ready to be consumed by the paired <see cref="IReader"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Flush/Fetch must be matched 1-to-1.</b>
        /// After a <see cref="IReader.Fetch"/> call the reader does not guarantee how many bytes
        /// are actually made available — this is an implementation detail of the underlying
        /// transport. Therefore the only reliable usage pattern is for the writer side and the
        /// reader side to perform an equal number of <see cref="Flush"/>/<see cref="IReader.Fetch"/>
        /// calls in lockstep. Mismatching counts is not itself a hard error, but the resulting
        /// behaviour is undefined.
        /// </para>
        /// <para>
        /// For in-memory implementations this method is a no-op and completes synchronously.
        /// For streaming implementations it frames and transmits the accumulated buffer to the
        /// underlying transport.
        /// </para>
        /// <para>
        /// This method must not be called while a section is open (i.e. after a
        /// <see cref="BeginSection"/> call that has not yet been matched by a corresponding
        /// <see cref="EndSection"/> call). Doing so will result in an
        /// <see cref="InvalidOperationException"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The writer is in an invalid state, such as when a section is currently open.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while flushing data.</exception>
        ValueTask Flush();
    }
}
