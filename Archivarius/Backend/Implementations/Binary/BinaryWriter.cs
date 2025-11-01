using System;
using System.Collections.Generic;
using System.Text;

namespace Archivarius.BinaryBackend
{
    public class BinaryWriter : IWriter
    {
        private int _capacity = 1024;
        private byte[] _buffer = new byte[1024];
        private int _size = 0;

        private bool _useSections = true;
        private readonly Stack<int> _sectionsStack = new Stack<int>();
        

        private void Grow(int value)
        {
            if (_size + value >= _capacity)
            {
                int newCapacity = _capacity;
                while (_size + value >= newCapacity)
                {
                    newCapacity *= 2;
                }

                byte[] newBuffer = new byte[newCapacity];
                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _size);

                _capacity = newCapacity;
                _buffer = newBuffer;
            }
        }

        public void Clear()
        {
            _size = 0;
            _sectionsStack.Clear();
        }

        public byte[] GetBuffer()
        {
            byte[] res = new byte[_size];
            Buffer.BlockCopy(_buffer, 0, res, 0, _size);
            return res;
        }
        public byte[] GetBufferUnsafe(out int size)
        {
            size = _size;
            return _buffer;
        }

        public byte[] TakeBuffer()
        {
            var res = GetBuffer();
            _size = 0;
            return res;
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            if (_size != 0)
            {
                return false;
            }
            _useSections = useSections;
            return true;
        }

        public void BeginSection()
        {
            if (_useSections)
            {
                _sectionsStack.Push(_size);
                WriteInt(0);
            }
        }

        public void EndSection()
        {
            if (_useSections)
            {
                int pos = _sectionsStack.Pop();
                int sectionSize = _size - pos - 4;

                IntToByte block = new IntToByte() { Value = sectionSize };
                _buffer[pos++] = block.Byte0;
                _buffer[pos++] = block.Byte1;
                _buffer[pos++] = block.Byte2;
                _buffer[pos++] = block.Byte3;
            }
        }

        public void WriteBool(bool value)
        {
            Grow(1);
            _buffer[_size++] = value ? (byte)1 : (byte)0;
        }
        
        public void WriteByte(byte value)
        {
            Grow(1);
            _buffer[_size++] = value;
        }

        public void WriteChar(char value)
        {
            CharToByte block = new CharToByte() {Value = value};
            Grow(2);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
        }

        public void WriteShort(short value)
        {
            ShortToByte block = new ShortToByte() {Value = value};
            Grow(2);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
        }

        public void WriteInt(int value)
        {
            IntToByte block = new IntToByte() {Value = value};
            Grow(4);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
            _buffer[_size++] = block.Byte2;
            _buffer[_size++] = block.Byte3;
        }

        public void WriteLong(long value)
        {
            LongToByte block = new LongToByte() {Value = value};
            Grow(8);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
            _buffer[_size++] = block.Byte2;
            _buffer[_size++] = block.Byte3;
            _buffer[_size++] = block.Byte4;
            _buffer[_size++] = block.Byte5;
            _buffer[_size++] = block.Byte6;
            _buffer[_size++] = block.Byte7;
        }

        public void WriteFloat(float value)
        {
            FloatToByte block = new FloatToByte() { Value = value };
            Grow(4);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
            _buffer[_size++] = block.Byte2;
            _buffer[_size++] = block.Byte3;
        }

        public void WriteDouble(double value)
        {
            DoubleToByte block = new DoubleToByte() { Value = value };
            Grow(8);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
            _buffer[_size++] = block.Byte2;
            _buffer[_size++] = block.Byte3;
            _buffer[_size++] = block.Byte4;
            _buffer[_size++] = block.Byte5;
            _buffer[_size++] = block.Byte6;
            _buffer[_size++] = block.Byte7;
        }

        public void WriteDecimal(decimal value)
        {
            DecimalToByte block = new DecimalToByte() { Value = value };
            Grow(16);
            _buffer[_size++] = block.Byte0;
            _buffer[_size++] = block.Byte1;
            _buffer[_size++] = block.Byte2;
            _buffer[_size++] = block.Byte3;
            _buffer[_size++] = block.Byte4;
            _buffer[_size++] = block.Byte5;
            _buffer[_size++] = block.Byte6;
            _buffer[_size++] = block.Byte7;
            _buffer[_size++] = block.Byte8;
            _buffer[_size++] = block.Byte9;
            _buffer[_size++] = block.Byte10;
            _buffer[_size++] = block.Byte11;
            _buffer[_size++] = block.Byte12;
            _buffer[_size++] = block.Byte13;
            _buffer[_size++] = block.Byte14;
            _buffer[_size++] = block.Byte15;
        }

        public void WriteString(string? value)
        {
            Grow(4);
            if (value == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _buffer[_size++] = map.Byte0;
                _buffer[_size++] = map.Byte1;
                _buffer[_size++] = map.Byte2;
                _buffer[_size++] = map.Byte3;
            }
            else
            {
                byte[] utfBytes = Encoding.UTF8.GetBytes(value);
                int count = utfBytes.Length;

                IntToByte map = new IntToByte() {Value = count + 1};
                _buffer[_size++] = map.Byte0;
                _buffer[_size++] = map.Byte1;
                _buffer[_size++] = map.Byte2;
                _buffer[_size++] = map.Byte3;

                Grow(count);
                Buffer.BlockCopy(utfBytes, 0, _buffer, _size, count);
                _size += count;
            }
        }

        public void WriteBytes(byte[]? bytes)
        {
            Grow(4);
            if (bytes == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _buffer[_size++] = map.Byte0;
                _buffer[_size++] = map.Byte1;
                _buffer[_size++] = map.Byte2;
                _buffer[_size++] = map.Byte3;
            }
            else
            {
                int count = bytes.Length;

                IntToByte map = new IntToByte() {Value = count + 1};
                _buffer[_size++] = map.Byte0;
                _buffer[_size++] = map.Byte1;
                _buffer[_size++] = map.Byte2;
                _buffer[_size++] = map.Byte3;

                Grow(count);
                Buffer.BlockCopy(bytes, 0, _buffer, _size, count);
                _size += count;
            }
        }
    }
}