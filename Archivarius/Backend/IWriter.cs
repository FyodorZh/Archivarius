namespace Archivarius
{
    public interface ILowLevelWriter
    {
        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteChar(char value);
        void WriteShort(short value);
        void WriteInt(int value);
        void WriteLong(long value);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteDecimal(decimal value);
        void WriteString(string? value);
        void WriteBytes(byte[]? value);
    }

    public interface IWriter : ILowLevelWriter
    {
        void BeginSection();
        void EndSection();
    }

    public static class ILowLevelWriter_Ext
    {
        public static void WriteSByte(this ILowLevelWriter writer, sbyte value)
        {
            writer.WriteByte(unchecked((byte)value));
        }

        public static void WriteUShort(this ILowLevelWriter writer, ushort value)
        {
            writer.WriteShort(unchecked((short)value));
        }

        public static void WriteUInt(this ILowLevelWriter writer, uint value)
        {
            writer.WriteInt(unchecked((int)value));
        }

        public static void WriteULong(this ILowLevelWriter writer, ulong value)
        {
            writer.WriteLong(unchecked((long)value));
        }
    }
}