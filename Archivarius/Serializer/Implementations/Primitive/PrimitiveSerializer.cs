using System;
using Archivarius.BinaryBackend;

namespace Archivarius
{
    public class PrimitiveSerializer : IPrimitiveSerializer
    {
        protected readonly IWriterBackend _writer;

        public bool IsWriter => true;
        public ILowLevelReader Reader => throw new InvalidOperationException();
        public ILowLevelWriter Writer => _writer.Writer;

        public PrimitiveSerializer(IWriterBackend writer)
        {
            _writer = writer;
        }

        public void Add(ref bool value)
        {
            _writer.Writer.WriteBool(value);
        }

        public void Add(ref byte value)
        {
            _writer.Writer.WriteByte(value);
        }

        public void Add(ref sbyte value)
        {
            byte uValue = unchecked((byte)value);
            _writer.Writer.WriteByte(uValue);
        }

        public void Add(ref char value)
        {
            _writer.Writer.WriteChar(value);
        }

        public void Add(ref short value)
        {
            _writer.Writer.WriteShort(value);
        }

        public void Add(ref ushort value)
        {
            _writer.Writer.WriteUShort(value);
        }

        public void Add(ref int value)
        {
            _writer.Writer.WriteInt(value);
        }

        public void Add(ref uint value)
        {
            _writer.Writer.WriteUInt(value);
        }

        public void Add(ref long value)
        {
            _writer.Writer.WriteLong(value);
        }

        public void Add(ref ulong value)
        {
            _writer.Writer.WriteULong(value);
        }

        public void Add(ref float value)
        {
            _writer.Writer.WriteFloat(value);
        }

        public void Add(ref double value)
        {
            _writer.Writer.WriteDouble(value);
        }
        
        public void Add(ref decimal value)
        {
            _writer.Writer.WriteDecimal(value);
        }

        public void Add(ref string? value)
        {
            _writer.Writer.WriteString(value);
        }
        
        public void Add(ref byte[]? value)
        {
            _writer.Writer.WriteArray(value);
        }

        public void Add(ref Guid value)
        {
            GuidToDecimal map = new GuidToDecimal() { Guid = value };
            _writer.Writer.WriteDecimal(map.Decimal);
        }
        
        public void Add(ref DateTime value)
        {
            long v = value.ToBinary();
            _writer.Writer.WriteLong(v);
        }
        
        public void Add(ref TimeSpan value)
        {
            long v = value.Ticks;
            _writer.Writer.WriteLong(v);
        }
    }
}