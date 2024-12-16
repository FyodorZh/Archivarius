using System;
using System.Collections.Generic;

namespace Archivarius.StructuredBinaryBackend
{
    public enum RecordType : byte
    {
        Unknown,
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
        Section
    }

    public readonly struct Record
    {
        public readonly RecordType Type;
        public readonly TypeAlias Value;
        public readonly string? Text;
        public readonly byte[]? Bytes;
        public readonly List<Record>? Section;

        public Record(byte value)
        {
            Type = RecordType.Byte;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(Char value)
        {
            Type = RecordType.Char;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(short value)
        {
            Type = RecordType.Short;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(int value)
        {
            Type = RecordType.Int;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(long value)
        {
            Type = RecordType.Long;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(float value)
        {
            Type = RecordType.Float;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(double value)
        {
            Type = RecordType.Double;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }
        
        public Record(decimal value)
        {
            Type = RecordType.Decimal;
            Value = value;
            Text = null;
            Bytes = null;
            Section = null;
        }

        public Record(string? value)
        {
            Type = RecordType.String;
            Value = 0;
            Text = value;
            Bytes = null;
            Section = null;
        }
        
        public Record(byte[]? value)
        {
            Type = RecordType.Array;
            Value = 0;
            Text = null;
            Bytes = value;
            Section = null;
        }

        public Record(List<Record> value)
        {
            Type = RecordType.Section;
            Value = 0;
            Text = null;
            Bytes = null;
            Section = value;
        }

        public Record Clone()
        {
            if (Section != null)
            {
                List<Record> list = new List<Record>(Section.Count);
                foreach (var record in Section)
                {
                    list.Add(record.Clone());
                }

                return new Record(list);
            }

            return this;
        }

        public void WriteTo(IWriter writer)
        {
            writer.WriteByte((byte)Type);
            switch (Type)
            {
                case RecordType.Byte:
                    writer.WriteByte(Value.ByteValue);
                    break;
                case RecordType.Char:
                    writer.WriteChar(Value.CharValue);
                    break;
                case RecordType.Short:
                    writer.WriteShort(Value.ShortValue);
                    break;
                case RecordType.Int:
                    writer.WriteInt(Value.IntValue);
                    break;
                case RecordType.Long:
                    writer.WriteLong(Value.LongValue);
                    break;
                case RecordType.Float:
                    writer.WriteFloat(Value.FloatValue);
                    break;
                case RecordType.Double:
                    writer.WriteDouble(Value.DoubleValue);
                    break;
                case RecordType.Decimal:
                    writer.WriteDecimal(Value.DecimalValue);
                    break;
                case RecordType.String:
                    writer.WriteString(Text);
                    break;
                case RecordType.Array:
                    writer.WriteBytes(Bytes);
                    break;
                case RecordType.Section:
                {
                    writer.WriteInt(Section!.Count);
                    foreach (var element in Section)
                    {
                        element.WriteTo(writer);
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Record ReadFrom(IReader reader)
        {
            RecordType type = (RecordType)reader.ReadByte();
            switch (type)
            {
                case RecordType.Byte:
                    return new Record(reader.ReadByte());
                case RecordType.Char:
                    return new Record(reader.ReadChar());
                case RecordType.Short:
                    return new Record(reader.ReadShort());
                case RecordType.Int:
                    return new Record(reader.ReadInt());
                case RecordType.Long:
                    return new Record(reader.ReadLong());
                case RecordType.Float:
                    return new Record(reader.ReadFloat());
                case RecordType.Double:
                    return new Record(reader.ReadDouble());
                case RecordType.Decimal:
                    return new Record(reader.ReadDecimal());
                case RecordType.String:
                    return new Record(reader.ReadString());
                case RecordType.Array:
                    return new Record(reader.ReadBytes());
                case RecordType.Section:
                {
                    int count = reader.ReadInt();
                    List<Record> section = new List<Record>(count);
                    for (int i = 0; i < count; ++i)
                    {
                        Record r = ReadFrom(reader);
                        section.Add(r);
                    }

                    return new Record(section);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case RecordType.Byte:
                    return "byte:" + Value.ByteValue;
                case RecordType.Char:
                    return "char:" + Value.CharValue;
                case RecordType.Short:
                    return "short:" + Value.ShortValue;
                case RecordType.Int:
                    return "int:" + Value.IntValue;
                case RecordType.Long:
                    return "long:" + Value.LongValue;
                case RecordType.Float:
                    return "float:" + Value.FloatValue;
                case RecordType.Double:
                    return "double:" + Value.DoubleValue;
                case RecordType.Decimal:
                    return "decimal:" + Value.DecimalValue;
                case RecordType.String:
                    return "text:" + Text;
                case RecordType.Array:
                    return "bytes:" + Bytes;
                case RecordType.Section:
                    return "Section[" + Section!.Count + "]";
                default:
                    return "INVALID_TYPE_" + Type;
            }
        }
    }
}