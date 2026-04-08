using System;

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
}
