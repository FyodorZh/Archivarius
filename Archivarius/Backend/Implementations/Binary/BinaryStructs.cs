using System.Runtime.InteropServices;

namespace Archivarius.BinaryBackend
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct CharToByte
    {
        [FieldOffset(0)] public char Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ShortToByte
    {
        [FieldOffset(0)] public short Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct IntToByte
    {
        [FieldOffset(0)] public int Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct FloatToByte
    {
        [FieldOffset(0)] public float Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct LongToByte
    {
        [FieldOffset(0)] public long Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct DoubleToByte
    {
        [FieldOffset(0)] public double Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct DecimalToByte
    {
        [FieldOffset(0)] public decimal Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;
        [FieldOffset(8)] public byte Byte8;
        [FieldOffset(9)] public byte Byte9;
        [FieldOffset(10)] public byte Byte10;
        [FieldOffset(11)] public byte Byte11;
        [FieldOffset(12)] public byte Byte12;
        [FieldOffset(13)] public byte Byte13;
        [FieldOffset(14)] public byte Byte14;
        [FieldOffset(15)] public byte Byte15;
    }
}