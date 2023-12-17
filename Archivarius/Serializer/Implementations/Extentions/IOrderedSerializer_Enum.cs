using System;

namespace Archivarius
{
    public static partial class IOrderedSerializer_Ext // Enums
    {
        /// <summary>
        /// Enum of any base type
        /// </summary>
        public static void Add<T>(this ISerializer serializer, ref T value)
            where T : struct, Enum
        {
            if (serializer.IsWriter)
            {
                switch (value.GetTypeCode())
                {
                    case TypeCode.Byte:
                        serializer.Writer.WriteByte((byte)(object)value);
                        break;
                    case TypeCode.Int16:
                        serializer.Writer.WriteShort((short)(object)value);
                        break;
                    case TypeCode.Int32:
                        serializer.Writer.WriteInt((int)(object)value);
                        break;
                    case TypeCode.Int64:
                        serializer.Writer.WriteLong((long)(object)value);
                        break;
                    case TypeCode.SByte:
                        serializer.Writer.WriteSByte((sbyte)(object)value);
                        break;
                    case TypeCode.UInt16:
                        serializer.Writer.WriteUShort((ushort)(object)value);
                        break;
                    case TypeCode.UInt32:
                        serializer.Writer.WriteUInt((uint)(object)value);
                        break;
                    case TypeCode.UInt64:
                        serializer.Writer.WriteULong((ulong)(object)value);
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
                        value = (T)(object)serializer.Reader.ReadByte();
                        break;
                    case TypeCode.Int16:
                        value = (T)(object)serializer.Reader.ReadShort();
                        break;
                    case TypeCode.Int32:
                        value = (T)(object)serializer.Reader.ReadInt();
                        break;
                    case TypeCode.Int64:
                        value = (T)(object)serializer.Reader.ReadLong();
                        break;
                    case TypeCode.SByte:
                        value = (T)(object)serializer.Reader.ReadSByte();
                        break;
                    case TypeCode.UInt16:
                        value = (T)(object)serializer.Reader.ReadUShort();
                        break;
                    case TypeCode.UInt32:
                        value = (T)(object)serializer.Reader.ReadUInt();
                        break;
                    case TypeCode.UInt64:
                        value = (T)(object)serializer.Reader.ReadULong();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}