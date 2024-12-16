using System;
using Archivarius.BinaryBackend;

namespace Archivarius
{
    public class PrimitiveDeserializer : IPrimitiveSerializer
    {
        protected readonly IReader _reader;
        
        public bool IsWriter => false;
        public ILowLevelReader Reader => _reader;
        public ILowLevelWriter Writer => throw new InvalidOperationException();

        public PrimitiveDeserializer(IReader reader)
        {
            _reader = reader;
        }

        public void Add(ref bool value)
        {
            value = _reader.ReadBool();
        }

        public void Add(ref byte value)
        {
            value = _reader.ReadByte();
        }

        public void Add(ref sbyte value)
        {
            var uValue = _reader.ReadByte();
            value = unchecked((sbyte)uValue);
        }

        public void Add(ref char value)
        {
            value = _reader.ReadChar();
        }

        public void Add(ref short value)
        {
            value = _reader.ReadShort();
        }

        public void Add(ref ushort value)
        {
            value = _reader.ReadChar();
        }

        public void Add(ref int value)
        {
            value = _reader.ReadInt();
        }

        public void Add(ref uint value)
        {
            var sValue = _reader.ReadInt();
            value = unchecked((uint)sValue);
        }

        public void Add(ref long value)
        {
            value = _reader.ReadLong();
        }

        public void Add(ref ulong value)
        {
            var sValue = _reader.ReadLong();
            value = unchecked((ulong)sValue);
        }

        public void Add(ref float value)
        {
            value = _reader.ReadFloat();
        }
        
        public void Add(ref double value)
        {
            value = _reader.ReadDouble();
        }
        
        public void Add(ref decimal value)
        {
            value = _reader.ReadDecimal();
        }

        public void Add(ref string? value)
        {
            value = _reader.ReadString();
        }
        
        public void Add(ref byte[]? value)
        {
            value = _reader.ReadBytes();
        }

        public void Add(ref Guid value)
        {
            GuidToDecimal map = new GuidToDecimal() { Decimal = _reader.ReadDecimal() };
            value = map.Guid;
        }
        
        public void Add(ref DateTime value)
        {
            long v = _reader.ReadLong();
            value = DateTime.FromBinary(v);
        }
    }
}