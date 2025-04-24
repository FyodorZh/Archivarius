using System;
using System.Collections.Generic;
using System.Text;

namespace Archivarius.BinaryBackend
{
    public class BinaryReader : IReader
    {
        private byte[] _buffer;

        private readonly Stack<int> _stackOfSections = new Stack<int>();
        private int _maxPosition;

        private int _position = 0;

        public BinaryReader(byte[] buffer, int count = -1)
        {
            _buffer = buffer;
            _maxPosition = count >= 0 ? count : buffer.Length;
        }

        public void Reset(byte[] newBuffer, int count = -1)
        {
            _buffer = newBuffer;
            _maxPosition = count >= 0 ? count : newBuffer.Length;
            _stackOfSections.Clear();
            _position = 0;
        }

        public void Reset()
        {
            _stackOfSections.Clear();
            _position = 0;
        }
        
        public void BeginSection()
        {
            int size = ReadInt();
            _stackOfSections.Push(_maxPosition);
            _maxPosition = _position + size;
        }

        public bool EndSection()
        {
            if (_maxPosition != _position)
            {
                _position = _maxPosition;
                _maxPosition = _stackOfSections.Pop();
                return false;
            }

            _maxPosition = _stackOfSections.Pop();
            return true;
        }

        private void Check(int grow)
        {
            if (_position + grow > _maxPosition)
            {
                throw new InvalidOperationException();
            }
        }

        public bool ReadBool()
        {
            Check(1);
            switch (_buffer[_position++])
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public byte ReadByte()
        {
            Check(1);
            return _buffer[_position++];
        }

        public char ReadChar()
        {
            Check(2);
            CharToByte block = new CharToByte
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++]
            };

            return block.Value;
        }

        public short ReadShort()
        {
            Check(2);
            ShortToByte block = new ShortToByte
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++]
            };

            return block.Value;
        }

        public int ReadInt()
        {
            Check(4);
            IntToByte block = new IntToByte
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++],
                Byte2 = _buffer[_position++],
                Byte3 = _buffer[_position++]
            };

            return block.Value;
        }

        public long ReadLong()
        {
            Check(8);
            LongToByte block = new LongToByte()
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++],
                Byte2 = _buffer[_position++],
                Byte3 = _buffer[_position++],
                Byte4 = _buffer[_position++],
                Byte5 = _buffer[_position++],
                Byte6 = _buffer[_position++],
                Byte7 = _buffer[_position++]
            };

            return block.Value;
        }

        public float ReadFloat()
        {
            Check(4);
            FloatToByte block = new FloatToByte()
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++],
                Byte2 = _buffer[_position++],
                Byte3 = _buffer[_position++],
            };

            return block.Value;
        }

        public double ReadDouble()
        {
            Check(8);
            DoubleToByte block = new DoubleToByte()
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++],
                Byte2 = _buffer[_position++],
                Byte3 = _buffer[_position++],
                Byte4 = _buffer[_position++],
                Byte5 = _buffer[_position++],
                Byte6 = _buffer[_position++],
                Byte7 = _buffer[_position++],
            };

            return block.Value;
        }

        public decimal ReadDecimal()
        {
            Check(16);
            DecimalToByte block = new DecimalToByte()
            {
                Byte0 = _buffer[_position++],
                Byte1 = _buffer[_position++],
                Byte2 = _buffer[_position++],
                Byte3 = _buffer[_position++],
                Byte4 = _buffer[_position++],
                Byte5 = _buffer[_position++],
                Byte6 = _buffer[_position++],
                Byte7 = _buffer[_position++],
                Byte8 = _buffer[_position++],
                Byte9 = _buffer[_position++],
                Byte10 = _buffer[_position++],
                Byte11 = _buffer[_position++],
                Byte12 = _buffer[_position++],
                Byte13 = _buffer[_position++],
                Byte14 = _buffer[_position++],
                Byte15 = _buffer[_position++],
            };

            return block.Value;
        }

        public string? ReadString()
        {
            int count = ReadInt();
            if (count == 0)
            {
                return null;
            }

            count -= 1;

            Check(count);

            string value = Encoding.UTF8.GetString(_buffer, _position, count);
            _position += count;
            return value;
        }
        
        public byte[]? ReadBytes()
        {
            int count = ReadInt();
            if (count == 0)
            {
                return null;
            }

            count -= 1;

            Check(count);

            byte[] value = new byte[count];
            Buffer.BlockCopy(_buffer, _position, value, 0, count);
            _position += count;
            return value;
        }
    }
}