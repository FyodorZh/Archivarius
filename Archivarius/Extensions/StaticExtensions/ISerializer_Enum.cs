using System;
using System.Runtime.CompilerServices;

namespace Archivarius
{
    public static class ISerializer_Enum
    {
        /// <summary>
        /// Enum of any base type
        /// </summary>
        public static void Add<T>(this ISerializerCore serializer, ref T value)
            where T : struct, Enum
        {
            if (serializer.IsWriter)
            {
                switch (value.GetTypeCode())
                {
                    case TypeCode.Byte:
                        serializer.Writer.WriteByte(Unsafe.As<T, byte>(ref value));
                        break;
                    case TypeCode.Int16:
                        serializer.Writer.WriteShort(Unsafe.As<T, short>(ref value));
                        break;
                    case TypeCode.Int32:
                        serializer.Writer.WriteInt(Unsafe.As<T, int>(ref value));
                        break;
                    case TypeCode.Int64:
                        serializer.Writer.WriteLong(Unsafe.As<T, long>(ref value));
                        break;
                    case TypeCode.SByte:
                        serializer.Writer.WriteSByte(Unsafe.As<T, sbyte>(ref value));
                        break;
                    case TypeCode.UInt16:
                        serializer.Writer.WriteUShort(Unsafe.As<T, ushort>(ref value));
                        break;
                    case TypeCode.UInt32:
                        serializer.Writer.WriteUInt(Unsafe.As<T, uint>(ref value));
                        break;
                    case TypeCode.UInt64:
                        serializer.Writer.WriteULong(Unsafe.As<T, ulong>(ref value));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                switch (value.GetTypeCode())
                {
                    case TypeCode.Byte:
                        { var tmp = serializer.Reader.ReadByte(); value = Unsafe.As<byte, T>(ref tmp); }
                        break;
                    case TypeCode.Int16:
                        { var tmp = serializer.Reader.ReadShort(); value = Unsafe.As<short, T>(ref tmp); }
                        break;
                    case TypeCode.Int32:
                        { var tmp = serializer.Reader.ReadInt(); value = Unsafe.As<int, T>(ref tmp); }
                        break;
                    case TypeCode.Int64:
                        { var tmp = serializer.Reader.ReadLong(); value = Unsafe.As<long, T>(ref tmp); }
                        break;
                    case TypeCode.SByte:
                        { var tmp = serializer.Reader.ReadSByte(); value = Unsafe.As<sbyte, T>(ref tmp); }
                        break;
                    case TypeCode.UInt16:
                        { var tmp = serializer.Reader.ReadUShort(); value = Unsafe.As<ushort, T>(ref tmp); }
                        break;
                    case TypeCode.UInt32:
                        { var tmp = serializer.Reader.ReadUInt(); value = Unsafe.As<uint, T>(ref tmp); }
                        break;
                    case TypeCode.UInt64:
                        { var tmp = serializer.Reader.ReadULong(); value = Unsafe.As<ulong, T>(ref tmp); }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}