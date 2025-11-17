using System;

namespace Archivarius.UnionDataListBackend
{
    public enum RecordType : byte
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
        public RecordType _type;
        public TypeAlias _value;
        private object? _object;
        
        public RecordType Type => _type;
        public TypeAlias Alias => _value;
        public string? Text => _object as string;
        public byte[]? Bytes => _object as byte[];
        public IDataStruct? DataStruct => _object as IDataStruct;

        public UnionData(bool value)
        {
            _type = RecordType.Bool;
            _value = value;
            _object = null;
        }
        
        public UnionData(byte value)
        {
            _type = RecordType.Byte;
            _value = value;
            _object = null;
        }

        public UnionData(Char value)
        {
            _type = RecordType.Char;
            _value = value;
            _object = null;
        }

        public UnionData(short value)
        {
            _type = RecordType.Short;
            _value = value;
            _object = null;
        }

        public UnionData(int value)
        {
            _type = RecordType.Int;
            _value = value;
            _object = null;
        }

        public UnionData(long value)
        {
            _type = RecordType.Long;
            _value = value;
            _object = null;
        }

        public UnionData(float value)
        {
            _type = RecordType.Float;
            _value = value;
            _object = null;
        }

        public UnionData(double value)
        {
            _type = RecordType.Double;
            _value = value;
            _object = null;
        }
        
        public UnionData(decimal value)
        {
            _type = RecordType.Decimal;
            _value = value;
            _object = null;
        }

        public UnionData(string? value)
        {
            _type = RecordType.String;
            _value = 0;
            _object = value;
        }
        
        public UnionData(byte[]? value)
        {
            _type = RecordType.Array;
            _value = 0;
            _object = value;
        }
        
        public UnionData(IDataStruct? value)
        {
            _type = RecordType.DataStruct;
            _value = 0;
            _object = value;
        }

        private void WriteTo(ILowLevelWriter writer)
        {
            switch (_type)
            {
                case RecordType.Bool:
                    writer.WriteBool(_value.BoolValue);
                    break;
                case RecordType.Byte:
                    writer.WriteByte(_value.ByteValue);
                    break;
                case RecordType.Char:
                    writer.WriteChar(_value.CharValue);
                    break;
                case RecordType.Short:
                    writer.WriteShort(_value.ShortValue);
                    break;
                case RecordType.Int:
                    writer.WriteInt(_value.IntValue);
                    break;
                case RecordType.Long:
                    writer.WriteLong(_value.LongValue);
                    break;
                case RecordType.Float:
                    writer.WriteFloat(_value.FloatValue);
                    break;
                case RecordType.Double:
                    writer.WriteDouble(_value.DoubleValue);
                    break;
                case RecordType.Decimal:
                    writer.WriteDecimal(_value.DecimalValue);
                    break;
                case RecordType.String:
                    writer.WriteString(Text);
                    break;
                case RecordType.Array:
                    writer.WriteBytes(Bytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReadFrom(RecordType type, ILowLevelReader reader)
        {
            switch (type)
            {
                case RecordType.Bool:
                    this = new UnionData(reader.ReadBool());
                    break;
                case RecordType.Byte:
                    this = new UnionData(reader.ReadByte());
                    return;
                case RecordType.Char:
                    this = new UnionData(reader.ReadChar());
                    return;
                case RecordType.Short:
                    this = new UnionData(reader.ReadShort());
                    return;
                case RecordType.Int:
                    this = new UnionData(reader.ReadInt());
                    return;
                case RecordType.Long:
                    this = new UnionData(reader.ReadLong());
                    return;
                case RecordType.Float:
                    this = new UnionData(reader.ReadFloat());
                    return;
                case RecordType.Double:
                    this = new UnionData(reader.ReadDouble());
                    return;
                case RecordType.Decimal:
                    this = new UnionData(reader.ReadDecimal());
                    return;
                case RecordType.String:
                    this = new UnionData(reader.ReadString());
                    return;
                case RecordType.Array:
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
                if (_type == RecordType.DataStruct)
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
                RecordType type = (RecordType)reader.ReadByte();
                if (type == RecordType.DataStruct)
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
                case RecordType.Bool:
                    return prefix + _value.BoolValue;
                case RecordType.Byte:
                    return prefix + _value.ByteValue;
                case RecordType.Char:
                    return prefix + _value.CharValue;
                case RecordType.Short:
                    return prefix + _value.ShortValue;
                case RecordType.Int:
                    return prefix + _value.IntValue;
                case RecordType.Long:
                    return prefix + _value.LongValue;
                case RecordType.Float:
                    return prefix + _value.FloatValue;
                case RecordType.Double:
                    return prefix + _value.DoubleValue;
                case RecordType.Decimal:
                    return prefix + _value.DecimalValue;
                case RecordType.String:
                    return prefix + Text;
                case RecordType.Array:
                    return prefix + Bytes;
                case RecordType.DataStruct:
                    return prefix + (_object?.GetType().FullName ?? "null");
                default:
                    return "INVALID_TYPE_" + _type;
            }
        }
    }
}