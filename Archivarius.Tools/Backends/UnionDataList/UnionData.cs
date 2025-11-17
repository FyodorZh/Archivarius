using System;

namespace Archivarius.UnionDataListBackend
{
    public enum UnionDataType : byte
    {
        Unknown,
        Bool,
        Byte,
        Char,
        Short,
        Int,
        Long,
        Float,
        Double,
        Decimal,
        String,
        Array,
        DataStruct
    }

    public struct UnionData : IDataStruct
    {
        public UnionDataType _type;
        public TypeAlias _value;
        private object? _object;
        
        public UnionDataType Type => _type;
        public TypeAlias Alias => _value;
        public string? Text => _object as string;
        public byte[]? Bytes => _object as byte[];
        public IDataStruct? DataStruct => _object as IDataStruct;
        
        public UnionData(bool value) 
        {
            _type = UnionDataType.Bool;
            _value = value;
            _object = null;
        }
        
        public UnionData(byte value)
        {
            _type = UnionDataType.Byte;
            _value = value;
            _object = null;
        }

        public UnionData(Char value)
        {
            _type = UnionDataType.Char;
            _value = value;
            _object = null;
        }

        public UnionData(short value)
        {
            _type = UnionDataType.Short;
            _value = value;
            _object = null;
        }

        public UnionData(int value)
        {
            _type = UnionDataType.Int;
            _value = value;
            _object = null;
        }

        public UnionData(long value)
        {
            _type = UnionDataType.Long;
            _value = value;
            _object = null;
        }

        public UnionData(float value)
        {
            _type = UnionDataType.Float;
            _value = value;
            _object = null;
        }

        public UnionData(double value)
        {
            _type = UnionDataType.Double;
            _value = value;
            _object = null;
        }
        
        public UnionData(decimal value)
        {
            _type = UnionDataType.Decimal;
            _value = value;
            _object = null;
        }

        public UnionData(string? value)
        {
            _type = UnionDataType.String;
            _value = 0;
            _object = value;
        }
        
        public UnionData(byte[]? value)
        {
            _type = UnionDataType.Array;
            _value = 0;
            _object = value;
        }
        
        public UnionData(IDataStruct? value)
        {
            _type = UnionDataType.DataStruct;
            _value = 0;
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
        public static implicit operator UnionData(int value)
        {
            return new UnionData(value);
        }
        public static implicit operator UnionData(long value)
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
                    writer.WriteBool(_value.BoolValue);
                    break;
                case UnionDataType.Byte:
                    writer.WriteByte(_value.ByteValue);
                    break;
                case UnionDataType.Char:
                    writer.WriteChar(_value.CharValue);
                    break;
                case UnionDataType.Short:
                    writer.WriteShort(_value.ShortValue);
                    break;
                case UnionDataType.Int:
                    writer.WriteInt(_value.IntValue);
                    break;
                case UnionDataType.Long:
                    writer.WriteLong(_value.LongValue);
                    break;
                case UnionDataType.Float:
                    writer.WriteFloat(_value.FloatValue);
                    break;
                case UnionDataType.Double:
                    writer.WriteDouble(_value.DoubleValue);
                    break;
                case UnionDataType.Decimal:
                    writer.WriteDecimal(_value.DecimalValue);
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
                case UnionDataType.Int:
                    this = new UnionData(reader.ReadInt());
                    return;
                case UnionDataType.Long:
                    this = new UnionData(reader.ReadLong());
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

        public override string ToString()
        {
            string prefix = _type + ":";
            switch (_type)
            {
                case UnionDataType.Bool:
                    return prefix + _value.BoolValue;
                case UnionDataType.Byte:
                    return prefix + _value.ByteValue;
                case UnionDataType.Char:
                    return prefix + _value.CharValue;
                case UnionDataType.Short:
                    return prefix + _value.ShortValue;
                case UnionDataType.Int:
                    return prefix + _value.IntValue;
                case UnionDataType.Long:
                    return prefix + _value.LongValue;
                case UnionDataType.Float:
                    return prefix + _value.FloatValue;
                case UnionDataType.Double:
                    return prefix + _value.DoubleValue;
                case UnionDataType.Decimal:
                    return prefix + _value.DecimalValue;
                case UnionDataType.String:
                    return prefix + Text;
                case UnionDataType.Array:
                    return prefix + Bytes;
                case UnionDataType.DataStruct:
                    return prefix + (_object?.GetType().FullName ?? "null");
                default:
                    return "INVALID_TYPE_" + _type;
            }
        }
    }
}