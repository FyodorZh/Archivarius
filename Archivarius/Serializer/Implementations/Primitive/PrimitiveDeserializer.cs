using System;
using Archivarius.BinaryBackend;

namespace Archivarius
{
    public class PrimitiveDeserializer : IPrimitiveSerializer
    {
        protected readonly IReaderBackend _reader;
        
        public bool IsWriter => false;
        public ILowLevelReader Reader => _reader.Reader;
        public ILowLevelWriter Writer => throw new InvalidOperationException();

        public PrimitiveDeserializer(IReaderBackend reader)
        {
            _reader = reader;
        }

        public void Add(ref bool value)
        {
            value = _reader.Reader.ReadBool();
        }

        public void Add(ref byte value)
        {
            value = _reader.Reader.ReadByte();
        }

        public void Add(ref sbyte value)
        {
            var uValue = _reader.Reader.ReadByte();
            value = unchecked((sbyte)uValue);
        }

        public void Add(ref char value)
        {
            value = _reader.Reader.ReadChar();
        }

        public void Add(ref short value)
        {
            value = _reader.Reader.ReadShort();
        }

        public void Add(ref ushort value)
        {
            value = _reader.Reader.ReadUShort();
        }

        public void Add(ref int value)
        {
            value = _reader.Reader.ReadInt();
        }

        public void Add(ref uint value)
        {
            value = _reader.Reader.ReadUInt();
        }

        public void Add(ref long value)
        {
            value = _reader.Reader.ReadLong();
        }

        public void Add(ref ulong value)
        {
            value = _reader.Reader.ReadULong();
        }

        public void Add(ref float value)
        {
            value = _reader.Reader.ReadFloat();
        }
        
        public void Add(ref double value)
        {
            value = _reader.Reader.ReadDouble();
        }
        
        public void Add(ref decimal value)
        {
            value = _reader.Reader.ReadDecimal();
        }

        public void Add(ref string? value)
        {
            value = _reader.Reader.ReadString();
        }
        
        public void Add(ref byte[]? value)
        {
            value = _reader.Reader.ReadArray();
        }

        public void Add(ref Guid value)
        {
            GuidToDecimal map = new GuidToDecimal() { Decimal = _reader.Reader.ReadDecimal() };
            value = map.Guid;
        }
        
        public void Add(ref DateTime value)
        {
            long v = _reader.Reader.ReadLong();
            value = DateTime.FromBinary(v);
        }
        
        public void Add(ref TimeSpan value)
        {
            long v = _reader.Reader.ReadLong();
            value = TimeSpan.FromTicks(v);
        }
    }
}