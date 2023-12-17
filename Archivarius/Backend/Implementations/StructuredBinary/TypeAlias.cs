using System.Runtime.InteropServices;

namespace Archivarius.StructuredBinaryBackend
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TypeAlias
    {
        [FieldOffset(0)] public byte ByteValue;
        [FieldOffset(0)] public char CharValue;
        [FieldOffset(0)] public short ShortValue;
        [FieldOffset(0)] public int IntValue;
        [FieldOffset(0)] public long LongValue;
        [FieldOffset(0)] public float FloatValue;
        [FieldOffset(0)] public double DoubleValue;

        public static implicit operator TypeAlias(byte value) => new TypeAlias() {ByteValue = value};
        public static implicit operator TypeAlias(char value) => new TypeAlias() {CharValue = value};
        public static implicit operator TypeAlias(short value) => new TypeAlias() {ShortValue = value};
        public static implicit operator TypeAlias(int value) => new TypeAlias() {IntValue = value};
        public static implicit operator TypeAlias(long value) => new TypeAlias() {LongValue = value};
        public static implicit operator TypeAlias(float value) => new TypeAlias() {FloatValue = value};
        public static implicit operator TypeAlias(double value) => new TypeAlias() {DoubleValue = value};
    }
}