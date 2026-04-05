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
        /// The stored data is not a valid <see cref="bool"/> encoding, or a type mismatch was
        /// detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteBool"/>
        bool ReadBool();

        /// <summary>Reads a <see cref="byte"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteByte"/>
        byte ReadByte();

        /// <summary>Reads an <see cref="sbyte"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteSByte"/>
        sbyte ReadSByte();

        /// <summary>Reads a <see cref="char"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteChar"/>
        char ReadChar();

        /// <summary>Reads a <see cref="short"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteShort"/>
        short ReadShort();

        /// <summary>Reads a <see cref="ushort"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadUShort"/>
        ushort ReadUShort();

        /// <summary>Reads an <see cref="int"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteInt"/>
        int ReadInt();

        /// <summary>Reads a <see cref="uint"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteUInt"/>
        uint ReadUInt();

        /// <summary>Reads a <see cref="long"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteLong"/>
        long ReadLong();

        /// <summary>Reads a <see cref="ulong"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteULong"/>
        ulong ReadULong();

        /// <summary>Reads a <see cref="float"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteFloat"/>
        float ReadFloat();

        /// <summary>Reads a <see cref="double"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteDouble"/>
        double ReadDouble();

        /// <summary>Reads a <see cref="decimal"/> value.</summary>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream does not contain enough data to read the value.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
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
        /// <exception cref="System.IO.InvalidDataException">
        /// The stored data is not a valid string encoding, or a type mismatch was detected by
        /// the implementation.
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
        /// <exception cref="System.IO.InvalidDataException">
        /// The stored data is not a valid byte-array encoding, or a type mismatch was detected
        /// by the implementation.
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
        /// <param name="count">The number of bytes to read. When zero, the method is a no-op (no bytes are consumed from the stream).</param>
        /// <remarks>
        /// In type-safe implementations, if the <paramref name="count"/> supplied here differs
        /// from the count that was passed to the paired <see cref="ILowLevelWriter.WriteBytes"/>,
        /// an <see cref="System.IO.InvalidDataException"/> is thrown <b>during this call</b>
        /// (not deferred to a section boundary or stream end).  Likewise, if a typed value was
        /// written (e.g. via <see cref="ILowLevelWriter.WriteInt"/>) but <see cref="ReadBytes"/>
        /// is called, an <see cref="System.IO.InvalidDataException"/> is thrown immediately.
        /// </remarks>
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
        /// <exception cref="System.IO.InvalidDataException">
        /// A type mismatch was detected by the implementation.
        /// </exception>
        /// <seealso cref="ILowLevelWriter.WriteBytes"/>
        void ReadBytes(byte[] dst, int offset, int count);
    }

    /// <summary>
    /// Extends <see cref="ILowLevelReader"/> with section management and data fetching.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Whether type mismatches are detected depends on the implementation.
    /// Implementations that support type safety will throw <see cref="System.IO.InvalidDataException"/>
    /// whenever a read method is called for a type that differs from the type that was written
    /// (e.g. calling <see cref="ILowLevelReader.ReadInt"/> when a <see cref="ILowLevelWriter.WriteFloat"/> was written, or
    /// calling <see cref="ILowLevelReader.ReadBytes"/> when a typed value was written). Type safety is strict:
    /// signed/unsigned variants are distinct types (<c>short</c> ≠ <c>ushort</c>, etc.).
     /// Implementations that do not guarantee type safety must still detect data corruption in a
     /// weaker form: if sections are enabled they must report unconsumed or overconsumed bytes at
     /// each <see cref="ISectionsCapability.EndSection"/>; if sections are disabled they must detect insufficient data
     /// at the end of the stream.
    /// </para>
    /// </remarks>
    public interface IReader : IAnyBackend, ILowLevelReader
    {
        /// <summary>
        /// Indicates whether the reader has reached the permanent end of the data stream —
        /// no further data will ever become available.
        /// </summary>
        /// <remarks>
        /// This property becomes <see langword="true"/> only after <see cref="Fetch"/> has
        /// returned <see langword="false"/>. Before that point the value is
        /// <see langword="false"/> regardless of how much data remains in the current buffer.
        /// </remarks>
        bool IsCompleted { get; }



        /// <summary>
        /// Asynchronously loads the next logical block of data from the underlying source,
        /// making it available for reading.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if data is (or may be) available and deserialization can
        /// proceed; <see langword="false"/> if no more data will ever be available and
        /// deserialization must stop.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Always call <see cref="Fetch"/> before reading any data.</b>
        /// Attempting to read data without a preceding <see cref="Fetch"/> call is undefined
        /// behaviour — the data may simply not be present yet, and the outcome is
        /// implementation-specific.
        /// </para>
        /// <para>
        /// <b>Flush/Fetch must be matched 1-to-1.</b>
        /// There is no guarantee about how many bytes a single <see cref="Fetch"/> call will
        /// make available; that is an implementation detail of the underlying transport. The
        /// only reliable usage pattern is therefore for both sides to perform an equal number
        /// of <see cref="IWriter.Flush"/>/<see cref="Fetch"/> calls in lockstep — one
        /// <see cref="IWriter.Flush"/> on the writer for every <see cref="Fetch"/> on the
        /// reader, in the same order. Mismatching counts is not itself a hard error, but the
        /// resulting behaviour is undefined.
        /// </para>
        /// <para>
        /// <b><see cref="IWriter.Complete"/> counts as one <see cref="IWriter.Flush"/>.</b>
        /// A writer that writes data and then calls <see cref="IWriter.Complete"/> (without a
        /// prior explicit <see cref="IWriter.Flush"/>) makes that data available via exactly
        /// one <see cref="Fetch"/> call. The subsequent <see cref="Fetch"/> will return
        /// <see langword="false"/>.
        /// </para>
        /// <para>
        /// <b>Blocking behaviour.</b> For streaming implementations, if <see cref="Fetch"/> is
        /// called before the paired writer has called <see cref="IWriter.Flush"/>, the call
        /// will suspend (await) until the data arrives.
        /// </para>
        /// <para>
        /// <b>No-data flushes.</b> <see cref="Fetch"/> may return <see langword="true"/> even
        /// when the paired <see cref="IWriter.Flush"/> wrote no additional data. Callers must
        /// not assume that <see langword="true"/> means new bytes are present.
        /// </para>
        /// <para>
        /// For in-memory implementations this method completes synchronously and simply
        /// reports whether the buffer has unread bytes remaining.
        /// For streaming implementations it reads the next framed block from the transport.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The reader is in an invalid state, such as when a section is currently open.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while fetching data.</exception>
        ValueTask<bool> Fetch();
    }
}
