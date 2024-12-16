using System;

namespace Archivarius
{
    public class PrimitiveSerializer : IPrimitiveSerializer
    {
        protected readonly IWriter _writer;

        public bool IsWriter => true;
        public ILowLevelReader Reader => throw new InvalidOperationException();
        public ILowLevelWriter Writer => _writer;

        public PrimitiveSerializer(IWriter writer)
        {
            _writer = writer;
        }

        public void Add(ref bool value)
        {
            _writer.WriteBool(value);
        }

        public void Add(ref byte value)
        {
            _writer.WriteByte(value);
        }

        public void Add(ref sbyte value)
        {
            byte uValue = unchecked((byte)value);
            _writer.WriteByte(uValue);
        }

        public void Add(ref char value)
        {
            _writer.WriteChar(value);
        }

        public void Add(ref short value)
        {
            _writer.WriteShort(value);
        }

        public void Add(ref ushort value)
        {
            _writer.WriteChar((char)value);
        }

        public void Add(ref int value)
        {
            _writer.WriteInt(value);
        }

        public void Add(ref uint value)
        {
            int sValue = unchecked((int)value);
            _writer.WriteInt(sValue);
        }

        public void Add(ref long value)
        {
            _writer.WriteLong(value);
        }

        public void Add(ref ulong value)
        {
            long sValue = unchecked((long)value);
            _writer.WriteLong(sValue);
        }

        public void Add(ref float value)
        {
            _writer.WriteFloat(value);
        }

        public void Add(ref double value)
        {
            _writer.WriteDouble(value);
        }
        
        public void Add(ref decimal value)
        {
            _writer.WriteDecimal(value);
        }

        public void Add(ref string? value)
        {
            _writer.WriteString(value);
        }
        
        public void Add(ref byte[]? value)
        {
            _writer.WriteBytes(value);
        }
    }
}