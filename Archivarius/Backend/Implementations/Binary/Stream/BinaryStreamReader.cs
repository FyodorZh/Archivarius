using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Archivarius.BinaryBackend
{
    public class BinaryStreamReader : IReader
    {
        private Stream _stream;

        private readonly Stack<long> _stackOfSections = new Stack<long>();
        private long _maxPosition;
        
        private bool _useSections;

        public BinaryStreamReader(Stream stream, long count = -1)
        {
            _stream = stream;
            _maxPosition = count >= 0 ? count : stream.Length;
        }

        public void Reset(Stream stream, long count = -1)
        {
            _stream = stream;
            _maxPosition = count >= 0 ? count : stream.Length;
            _stackOfSections.Clear();
        }

        public void Reset()
        {
            _stackOfSections.Clear();
            _stream.Position = 0;
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            _useSections = useSections;
            return true;
        }

        public void BeginSection()
        {
            if (_useSections)
            {
                long size = ReadInt();
                _stackOfSections.Push(_maxPosition);
                _maxPosition = _stream.Position + size;
            }
        }

        public bool EndSection()
        {
            if (!_useSections)
            {
                return true;
            }
            
            if (_maxPosition != _stream.Position)
            {
                _stream.Position = _maxPosition;
                _maxPosition = _stackOfSections.Pop();
                return false;
            }

            _maxPosition = _stackOfSections.Pop();
            return true;
        }

        private void Check(int grow)
        {
            if (_stream.Position + grow > _maxPosition)
            {
                throw new InvalidOperationException();
            }
        }

        public bool ReadBool()
        {
            Check(1);
            switch (_stream.ReadByte())
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
            return (byte)_stream.ReadByte();
        }

        public char ReadChar()
        {
            Check(2);
            CharToByte block = new CharToByte
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte()
            };

            return block.Value;
        }

        public short ReadShort()
        {
            Check(2);
            ShortToByte block = new ShortToByte
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte()
            };

            return block.Value;
        }

        public int ReadInt()
        {
            Check(4);
            IntToByte block = new IntToByte
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte(),
                Byte2 = (byte)_stream.ReadByte(),
                Byte3 = (byte)_stream.ReadByte()
            };

            return block.Value;
        }

        public long ReadLong()
        {
            Check(8);
            LongToByte block = new LongToByte()
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte(),
                Byte2 = (byte)_stream.ReadByte(),
                Byte3 = (byte)_stream.ReadByte(),
                Byte4 = (byte)_stream.ReadByte(),
                Byte5 = (byte)_stream.ReadByte(),
                Byte6 = (byte)_stream.ReadByte(),
                Byte7 = (byte)_stream.ReadByte()
            };

            return block.Value;
        }

        public float ReadFloat()
        {
            Check(4);
            FloatToByte block = new FloatToByte()
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte(),
                Byte2 = (byte)_stream.ReadByte(),
                Byte3 = (byte)_stream.ReadByte(),
            };

            return block.Value;
        }

        public double ReadDouble()
        {
            Check(8);
            DoubleToByte block = new DoubleToByte()
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte(),
                Byte2 = (byte)_stream.ReadByte(),
                Byte3 = (byte)_stream.ReadByte(),
                Byte4 = (byte)_stream.ReadByte(),
                Byte5 = (byte)_stream.ReadByte(),
                Byte6 = (byte)_stream.ReadByte(),
                Byte7 = (byte)_stream.ReadByte(),
            };

            return block.Value;
        }

        public decimal ReadDecimal()
        {
            Check(16);
            DecimalToByte block = new DecimalToByte()
            {
                Byte0 = (byte)_stream.ReadByte(),
                Byte1 = (byte)_stream.ReadByte(),
                Byte2 = (byte)_stream.ReadByte(),
                Byte3 = (byte)_stream.ReadByte(),
                Byte4 = (byte)_stream.ReadByte(),
                Byte5 = (byte)_stream.ReadByte(),
                Byte6 = (byte)_stream.ReadByte(),
                Byte7 = (byte)_stream.ReadByte(),
                Byte8 = (byte)_stream.ReadByte(),
                Byte9 = (byte)_stream.ReadByte(),
                Byte10 = (byte)_stream.ReadByte(),
                Byte11 = (byte)_stream.ReadByte(),
                Byte12 = (byte)_stream.ReadByte(),
                Byte13 = (byte)_stream.ReadByte(),
                Byte14 = (byte)_stream.ReadByte(),
                Byte15 = (byte)_stream.ReadByte(),
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

            byte[] bytes = new byte[count];
            var fact = _stream.Read(bytes, 0, count);
            if (fact != count)
            {
                throw new EndOfStreamException();
            }
            
            string value = Encoding.UTF8.GetString(bytes, 0, count);
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
            var fact = _stream.Read(value, 0, count);
            if (fact != count)
            {
                throw new EndOfStreamException();
            }
            return value;
        }
    }
}