using System;

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
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadBool"/>
        void WriteBool(bool value);

        /// <summary>Writes a <see cref="byte"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadByte"/>
        void WriteByte(byte value);

        /// <summary>Writes an <see cref="sbyte"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadSByte"/>
        void WriteSByte(sbyte value);

        /// <summary>Writes a <see cref="char"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadChar"/>
        void WriteChar(char value);

        /// <summary>Writes a <see cref="short"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadShort"/>
        void WriteShort(short value);

        /// <summary>Writes a <see cref="ushort"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadUShort"/>
        void WriteUShort(ushort value);

        /// <summary>Writes an <see cref="int"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadInt"/>
        void WriteInt(int value);

        /// <summary>Writes a <see cref="uint"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadUInt"/>
        void WriteUInt(uint value);

        /// <summary>Writes a <see cref="long"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadLong"/>
        void WriteLong(long value);

        /// <summary>Writes a <see cref="ulong"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadULong"/>
        void WriteULong(ulong value);

        /// <summary>Writes a <see cref="float"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadFloat"/>
        void WriteFloat(float value);

        /// <summary>Writes a <see cref="double"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadDouble"/>
        void WriteDouble(double value);

        /// <summary>Writes a <see cref="decimal"/> value.</summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadDecimal"/>
        void WriteDecimal(decimal value);

        /// <summary>
        /// Writes a nullable UTF-8 encoded <see cref="string"/> value.
        /// A <see langword="null"/> value is encoded as a distinct sentinel so that
        /// <see cref="ILowLevelReader.ReadString"/> can faithfully return <see langword="null"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadString"/>
        void WriteString(string? value);

        /// <summary>
        /// Writes a nullable byte array.
        /// A <see langword="null"/> value is encoded as a distinct sentinel so that
        /// <see cref="ILowLevelReader.ReadArray"/> can faithfully return <see langword="null"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
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
        /// <remarks>
        /// In type-safe implementations the written <paramref name="count"/> is stored alongside
        /// the data. If the paired <see cref="ILowLevelReader.ReadBytes"/> call specifies a
        /// different count, or if a typed read method is called instead, an
        /// <see cref="System.IO.InvalidDataException"/> is thrown <b>during the read call</b>
        /// (not deferred to a section boundary or stream end).
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="offset"/> + <paramref name="count"/> exceeds the length of <paramref name="value"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The writer has already been completed via <see cref="IWriterBackend.Complete"/>.
        /// </exception>
        /// <seealso cref="ILowLevelReader.ReadBytes"/>
        void WriteBytes(byte[] value, int offset, int count);
    }
}
