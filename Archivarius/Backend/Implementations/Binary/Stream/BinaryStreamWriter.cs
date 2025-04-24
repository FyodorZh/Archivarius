using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Archivarius.BinaryBackend
{
    public class BinaryStreamWriter : IWriter
    {
        private readonly Stream _stream;
        
        public Stream BaseStream => _stream;

        public BinaryStreamWriter(Stream stream)
        {
            _stream = stream;
        }
        
        public bool TrySetSectionUsage(bool useSections)
        {
            if (useSections)
            {
                return false;
            }

            return true;
        }

        public void BeginSection()
        {
            // DO NOTHING
        }

        public void EndSection()
        {
            // DO NOTHING
        }

        public void WriteBool(bool value)
        {
            _stream.WriteByte(value ? (byte)1 : (byte)0);
        }
        
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public void WriteChar(char value)
        {
            CharToByte block = new CharToByte() {Value = value};
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
        }

        public void WriteShort(short value)
        {
            ShortToByte block = new ShortToByte() {Value = value};
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
        }

        public void WriteInt(int value)
        {
            IntToByte block = new IntToByte() {Value = value};
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
            _stream.WriteByte(block.Byte2);
            _stream.WriteByte(block.Byte3);
        }

        public void WriteLong(long value)
        {
            LongToByte block = new LongToByte() {Value = value};
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
            _stream.WriteByte(block.Byte2);
            _stream.WriteByte(block.Byte3);
            _stream.WriteByte(block.Byte4);
            _stream.WriteByte(block.Byte5);
            _stream.WriteByte(block.Byte6);
            _stream.WriteByte(block.Byte7);
        }

        public void WriteFloat(float value)
        {
            FloatToByte block = new FloatToByte() { Value = value };
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
            _stream.WriteByte(block.Byte2);
            _stream.WriteByte(block.Byte3);
        }

        public void WriteDouble(double value)
        {
            DoubleToByte block = new DoubleToByte() { Value = value };
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
            _stream.WriteByte(block.Byte2);
            _stream.WriteByte(block.Byte3);
            _stream.WriteByte(block.Byte4);
            _stream.WriteByte(block.Byte5);
            _stream.WriteByte(block.Byte6);
            _stream.WriteByte(block.Byte7);
        }

        public void WriteDecimal(decimal value)
        {
            DecimalToByte block = new DecimalToByte() { Value = value };
            _stream.WriteByte(block.Byte0);
            _stream.WriteByte(block.Byte1);
            _stream.WriteByte(block.Byte2);
            _stream.WriteByte(block.Byte3);
            _stream.WriteByte(block.Byte4);
            _stream.WriteByte(block.Byte5);
            _stream.WriteByte(block.Byte6);
            _stream.WriteByte(block.Byte7);
            _stream.WriteByte(block.Byte8);
            _stream.WriteByte(block.Byte9);
            _stream.WriteByte(block.Byte10);
            _stream.WriteByte(block.Byte11);
            _stream.WriteByte(block.Byte12);
            _stream.WriteByte(block.Byte13);
            _stream.WriteByte(block.Byte14);
            _stream.WriteByte(block.Byte15);
        }

        public void WriteString(string? value)
        {
            if (value == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _stream.WriteByte(map.Byte0);
                _stream.WriteByte(map.Byte1);
                _stream.WriteByte(map.Byte2);
                _stream.WriteByte(map.Byte3);
            }
            else
            {
                byte[] utfBytes = Encoding.UTF8.GetBytes(value);
                int count = utfBytes.Length;

                IntToByte map = new IntToByte() {Value = count + 1};
                _stream.WriteByte(map.Byte0);
                _stream.WriteByte(map.Byte1);
                _stream.WriteByte(map.Byte2);
                _stream.WriteByte(map.Byte3);

                _stream.Write(utfBytes, 0, count);
            }
        }

        public void WriteBytes(byte[]? bytes)
        {
            if (bytes == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _stream.WriteByte(map.Byte0);
                _stream.WriteByte(map.Byte1);
                _stream.WriteByte(map.Byte2);
                _stream.WriteByte(map.Byte3);
            }
            else
            {
                int count = bytes.Length;

                IntToByte map = new IntToByte() {Value = count + 1};
                _stream.WriteByte(map.Byte0);
                _stream.WriteByte(map.Byte1);
                _stream.WriteByte(map.Byte2);
                _stream.WriteByte(map.Byte3);

                _stream.Write(bytes, 0, count);
            }
        }
    }
}