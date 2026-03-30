using System;
using System.Threading.Tasks;

namespace Archivarius
{
    /// <summary>
    /// Defines the low-level primitive read operations.
    /// Each method has a corresponding write method in <see cref="ILowLevelWriter"/>
    /// that must have been called in the same order to correctly serialize the data.
    /// </summary>
    public interface ILowLevelReader
    {
        /// <summary>Reads a <see cref="bool"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// The stored byte is not a valid boolean encoding.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteBool"/>
        bool ReadBool();

        /// <summary>Reads a <see cref="byte"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteByte"/>
        byte ReadByte();

        /// <summary>Reads an <see cref="sbyte"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteSByte"/>
        sbyte ReadSByte();

        /// <summary>Reads a <see cref="char"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteChar"/>
        char ReadChar();

        /// <summary>Reads a <see cref="short"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteShort"/>
        short ReadShort();

        /// <summary>Reads a <see cref="ushort"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadUShort"/>
        ushort ReadUShort();

        /// <summary>Reads an <see cref="int"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteInt"/>
        int ReadInt();

        /// <summary>Reads a <see cref="uint"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteUInt"/>
        uint ReadUInt();

        /// <summary>Reads a <see cref="long"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteLong"/>
        long ReadLong();

        /// <summary>Reads a <see cref="ulong"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteULong"/>
        ulong ReadULong();

        /// <summary>Reads a <see cref="float"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteFloat"/>
        float ReadFloat();

        /// <summary>Reads a <see cref="double"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteDouble"/>
        double ReadDouble();

        /// <summary>Reads a <see cref="decimal"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteDecimal"/>
        decimal ReadDecimal();

        /// <summary>
        /// Reads a nullable UTF-8 encoded <see cref="string"/> value written by
        /// <see cref="ILowLevelWriter.WriteString"/>. Returns <see langword="null"/> if
        /// a <see langword="null"/> sentinel was written.
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteString"/>
        string? ReadString();

        /// <summary>
        /// Reads a nullable byte array written by <see cref="ILowLevelWriter.WriteArray"/>.
        /// Returns <see langword="null"/> if a <see langword="null"/> sentinel was written.
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteArray"/>
        byte[]? ReadArray();

        /// <summary>
        /// Reads <paramref name="count"/> raw bytes into <paramref name="dst"/> starting at
        /// <paramref name="offset"/>. Must be paired with a corresponding
        /// <see cref="ILowLevelWriter.WriteBytes"/> call that wrote the same number of bytes.
        /// </summary>
        /// <param name="dst">The destination buffer.</param>
        /// <param name="offset">The zero-based start index within <paramref name="dst"/>.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dst"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="offset"/> + <paramref name="count"/> exceeds the length of <paramref name="dst"/>.
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read <paramref name="count"/> bytes.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteBytes"/>
        void ReadBytes(byte[] dst, int offset, int count);
    }

    /// <summary>
    /// Extends <see cref="ILowLevelReader"/> with section management and data fetching.
    /// </summary>
    public interface IReader : ILowLevelReader
    {
        /// <summary>
        /// Attempts to enable or disable logical section markers for deserialization.
        /// Must be configured to the same value that was used on the paired <see cref="IWriter"/>.
        /// </summary>
        /// <param name="useSections">
        /// <see langword="true"/> to expect section length headers in the stream;
        /// <see langword="false"/> to omit them.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the mode was successfully applied.
        /// <see langword="false"/> if the request was rejected — either because data has already
        /// been read, or because the implementation does not support the requested mode.
        /// </returns>
        /// <remarks>
        /// Must be called before any data is read from the stream.
        /// </remarks>
        bool TrySetSectionUsage(bool useSections);

        /// <summary>
        /// Marks the beginning of a logical section.
        /// When sections are enabled, reads the section's byte-length header written by the
        /// paired <see cref="IWriter.BeginSection"/> and constrains subsequent reads to that
        /// section's boundary.
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
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the section header.
        /// </exception>
        void BeginSection();

        /// <summary>
        /// Marks the end of the current logical section and restores the previous stream boundary.
        /// If the section contained more bytes than were consumed (e.g. the data was produced by
        /// a newer writer with extra fields), the reader automatically advances past the remaining
        /// bytes to maintain stream alignment.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the section was fully consumed (no bytes were skipped).
        /// <see langword="false"/> if the section contained unread bytes that were automatically skipped.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Must be preceded by a matching <see cref="BeginSection"/> call.
        /// Sections must be closed in reverse order of opening (LIFO).
        /// </para>
        /// <para>
        /// Can be called regardless of whether sections are enabled via
        /// <see cref="TrySetSectionUsage"/>. When sections are disabled this method is a no-op
        /// and always returns <see langword="true"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Called without a matching open <see cref="BeginSection"/>.
        /// </exception>
        bool EndSection();

        /// <summary>
        /// Asynchronously loads the next logical block of data from the underlying source,
        /// making it available for reading.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if data is available and deserialization can proceed;
        /// <see langword="false"/> if no more data is available and deserialization must stop.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Each <see cref="Fetch"/> call must correspond to exactly one prior
        /// <see cref="IWriter.Flush"/> call on the paired writer, and vice-versa.
        /// </para>
        /// <para>
        /// For in-memory implementations this method completes synchronously and simply
        /// reports whether the buffer has unread bytes remaining.
        /// For streaming implementations it reads the next framed block from the transport.
        /// </para>
        /// </remarks>
        /// <exception cref="System.IO.IOException">An I/O error occurred while fetching data.</exception>
        ValueTask<bool> Fetch();
    }
}
