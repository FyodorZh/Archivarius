using System;
using System.Globalization;
using System.Linq;

namespace Archivarius
{
    public struct UnionData : IDataStruct, IEquatable<UnionData>
    {
        private UnionDataType _type;
        private UnionDataMemoryAlias _alias;
        private object? _object;
        
        public UnionDataType Type => _type;
        public UnionDataMemoryAlias Alias => _alias;
        public string? Text => _object as string;
        public byte[]? Bytes => _object as byte[];
        public IDataStruct? DataStruct => _object as IDataStruct;
        
        public UnionData(bool value) 
        {
            _type = UnionDataType.Bool;
            _alias = value;
            _object = null;
        }
        
        public UnionData(byte value)
        {
            _type = UnionDataType.Byte;
            _alias = value;
            _object = null;
        }

        public UnionData(Char value)
        {
            _type = UnionDataType.Char;
            _alias = value;
            _object = null;
        }

        public UnionData(short value)
        {
            _type = UnionDataType.Short;
            _alias = value;
            _object = null;
        }
        
        public UnionData(ushort value)
        {
            _type = UnionDataType.UShort;
            _alias = value;
            _object = null;
        }

        public UnionData(int value)
        {
            _type = UnionDataType.Int;
            _alias = value;
            _object = null;
        }
        
        public UnionData(uint value)
        {
            _type = UnionDataType.UInt;
            _alias = value;
            _object = null;
        }

        public UnionData(long value)
        {
            _type = UnionDataType.Long;
            _alias = value;
            _object = null;
        }
        
        public UnionData(ulong value)
        {
            _type = UnionDataType.ULong;
            _alias = value;
            _object = null;
        }

        public UnionData(float value)
        {
            _type = UnionDataType.Float;
            _alias = value;
            _object = null;
        }

        public UnionData(double value)
        {
            _type = UnionDataType.Double;
            _alias = value;
            _object = null;
        }
        
        public UnionData(decimal value)
        {
            _type = UnionDataType.Decimal;
            _alias = value;
            _object = null;
        }

        public UnionData(string? value)
        {
            _type = UnionDataType.String;
            _alias = 0;
            _object = value;
        }
        
        public UnionData(byte[]? value)
        {
            _type = UnionDataType.Array;
            _alias = 0;
            _object = value;
        }
        
        public UnionData(IDataStruct? value)
        {
            _type = UnionDataType.DataStruct;
            _alias = 0;
            _object = value;
        }
        
        public static implicit operator UnionData(bool value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(byte value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(char value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(short value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(ushort value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(int value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(uint value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(long value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(ulong value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(float value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(double value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(decimal value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(string? value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(byte[]? value)
        {
            return new UnionData(value);
        }

        private void WriteTo(ILowLevelWriter writer)
        {
            switch (_type)
            {
                case UnionDataType.Bool:
                    writer.WriteBool(_alias.BoolValue);
                    break;
                case UnionDataType.Byte:
                    writer.WriteByte(_alias.ByteValue);
                    break;
                case UnionDataType.Char:
                    writer.WriteChar(_alias.CharValue);
                    break;
                case UnionDataType.Short:
                    writer.WriteShort(_alias.ShortValue);
                    break;
                case UnionDataType.UShort:
                    writer.WriteUShort(_alias.UShortValue);
                    break;
                case UnionDataType.Int:
                    writer.WriteInt(_alias.IntValue);
                    break;
                case UnionDataType.UInt:
                    writer.WriteUInt(_alias.UIntValue);
                    break;
                case UnionDataType.Long:
                    writer.WriteLong(_alias.LongValue);
                    break;
                case UnionDataType.ULong:
                    writer.WriteULong(_alias.ULongValue);
                    break;
                case UnionDataType.Float:
                    writer.WriteFloat(_alias.FloatValue);
                    break;
                case UnionDataType.Double:
                    writer.WriteDouble(_alias.DoubleValue);
                    break;
                case UnionDataType.Decimal:
                    writer.WriteDecimal(_alias.DecimalValue);
                    break;
                case UnionDataType.String:
                    writer.WriteString(Text);
                    break;
                case UnionDataType.Array:
                    writer.WriteBytes(Bytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReadFrom(UnionDataType type, ILowLevelReader reader)
        {
            switch (type)
            {
                case UnionDataType.Bool:
                    this = new UnionData(reader.ReadBool());
                    break;
                case UnionDataType.Byte:
                    this = new UnionData(reader.ReadByte());
                    return;
                case UnionDataType.Char:
                    this = new UnionData(reader.ReadChar());
                    return;
                case UnionDataType.Short:
                    this = new UnionData(reader.ReadShort());
                    return;
                case UnionDataType.UShort:
                    this = new UnionData(reader.ReadUShort());
                    return;
                case UnionDataType.Int:
                    this = new UnionData(reader.ReadInt());
                    return;
                case UnionDataType.UInt:
                    this = new UnionData(reader.ReadUInt());
                    return;
                case UnionDataType.Long:
                    this = new UnionData(reader.ReadLong());
                    return;
                case UnionDataType.ULong:
                    this = new UnionData(reader.ReadULong());
                    return;
                case UnionDataType.Float:
                    this = new UnionData(reader.ReadFloat());
                    return;
                case UnionDataType.Double:
                    this = new UnionData(reader.ReadDouble());
                    return;
                case UnionDataType.Decimal:
                    this = new UnionData(reader.ReadDecimal());
                    return;
                case UnionDataType.String:
                    this = new UnionData(reader.ReadString());
                    return;
                case UnionDataType.Array:
                    this = new UnionData(reader.ReadBytes());
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Serialize(ISerializer serializer)
        {
            if (serializer.IsWriter)
            {
                serializer.Writer.WriteByte((byte)_type);
                if (_type == UnionDataType.DataStruct)
                {
                    IDataStruct? dataStruct = _object as IDataStruct;
                    serializer.AddClass(ref dataStruct);
                }
                else
                {
                    WriteTo(serializer.Writer);
                }
            }
            else
            {
                var reader = serializer.Reader;
                UnionDataType type = (UnionDataType)reader.ReadByte();
                if (type == UnionDataType.DataStruct)
                {
                    IDataStruct? dataStruct = null;
                    serializer.AddClass(ref dataStruct);
                    _object = dataStruct;
                    _type = type;
                }
                else
                {
                    ReadFrom(type, reader);    
                }
            }
        }
        
        public bool Equals(UnionData other)
        {
            switch (_type)
            {
                case UnionDataType.Bool:
                case UnionDataType.Byte:
                    return _alias.Equals1(other._alias);
                case UnionDataType.Char:
                case UnionDataType.Short:
                case UnionDataType.UShort:
                    return _alias.Equals2(other._alias);
                case UnionDataType.Int:
                case UnionDataType.UInt:
                case UnionDataType.Float:
                    return _alias.Equals4(other._alias);
                case UnionDataType.Long:
                case UnionDataType.ULong:
                case UnionDataType.Double:
                    return _alias.Equals8(other._alias);
                case UnionDataType.Decimal: 
                    return _alias.Equals16(other._alias);
                case UnionDataType.String:
                    return Text == other.Text;
                case UnionDataType.Array:
                {
                    if ((Bytes == null) != (other.Bytes == null))
                    {
                        return false;
                    }
                    if (Bytes == null)
                    {
                        return true;
                    }
                    return Bytes.SequenceEqual(other.Bytes!);
                }
                case UnionDataType.DataStruct:
                {
                    if (ReferenceEquals(DataStruct, other.DataStruct)) 
                        return true;
                    throw new InvalidOperationException("IDataStruct comparison not supported");
                }
                default: 
                    return false;
            }
        }
        
        public string ValueToString()
        {
            switch (_type)
            {
                case UnionDataType.Bool: return _alias.BoolValue.ToString();
                case UnionDataType.Byte: return _alias.ByteValue.ToString();
                case UnionDataType.Char: return _alias.CharValue.ToString();
                case UnionDataType.Short: return _alias.ShortValue.ToString();
                case UnionDataType.Int: return _alias.IntValue.ToString();
                case UnionDataType.Long: return _alias.LongValue.ToString();
                case UnionDataType.Float: return _alias.FloatValue.ToString(CultureInfo.InvariantCulture);
                case UnionDataType.Double: return _alias.DoubleValue.ToString(CultureInfo.InvariantCulture);
                case UnionDataType.Decimal: return _alias.DecimalValue.ToString(CultureInfo.InvariantCulture);
                case UnionDataType.String: return Text ?? "null";
                case UnionDataType.Array: return Bytes != null ? ("[" + string.Join(",", Bytes) + "]") : "null";
                case UnionDataType.DataStruct: return _object?.GetType().FullName ?? "null";
                default: return "INVALID";
            }
        }

        public override string ToString()
        {
            return _type + ":" + ValueToString();
        }
    }
}