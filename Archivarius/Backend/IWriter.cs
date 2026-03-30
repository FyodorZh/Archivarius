using System;
using System.Threading.Tasks;

namespace Archivarius
{
    public interface ILowLevelWriter
    {
        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteSByte(sbyte value);
        void WriteChar(char value);
        void WriteShort(short value);
        void WriteUShort(ushort value);
        void WriteInt(int value);
        void WriteUInt(uint value);
        void WriteLong(long value);
        void WriteULong(ulong value);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteDecimal(decimal value);
        void WriteString(string? value);
        void WriteArray(byte[]? value);
        void WriteBytes(byte[] value, int offset, int count);
    }

    public interface IWriter : ILowLevelWriter
    {
        ValueTask Flush();
        bool TrySetSectionUsage(bool useSections);
        void BeginSection();
        void EndSection();
    }
}

