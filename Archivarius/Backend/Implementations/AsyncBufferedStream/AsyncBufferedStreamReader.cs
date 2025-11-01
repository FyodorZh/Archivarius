using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Archivarius.Internals;

namespace Archivarius.BinaryBackend
{
    public class AsyncBufferedStreamReader : IReader
    {
        private readonly Stream _source;
        private readonly ByteStream _buffer;

        private readonly byte[] _readBlockBuffer;

        private readonly Stack<long> _stackOfSections = new Stack<long>();
        private long _maxPosition;

        public AsyncBufferedStreamReader(Stream source, int readBlockSize)
        {
            _source = source;
            _readBlockBuffer = new byte[readBlockSize];
            
            _buffer = new ByteStream(128);
            
            _maxPosition = long.MaxValue;
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            if (useSections)
            {
                return true;
            }
            throw new InvalidOperationException();
        }

        public void BeginSection()
        {
            int size = ReadInt();
            _stackOfSections.Push(_maxPosition);
            _maxPosition = _buffer.LogicalPosition + size;
        }

        public async ValueTask BeginSectionAsync()
        {
            int size = ReadInt();
            _stackOfSections.Push(_maxPosition);
            _maxPosition = _buffer.LogicalPosition + size;
            
            int count;
            if (_buffer.AvailableToProcess < size &&
                (count = await _source.ReadAsync(_readBlockBuffer, 0, _readBlockBuffer.Length)) > 0)
            {
                _buffer.Put(_readBlockBuffer, 0, count);
            }
        }

        public bool EndSection()
        {
            if (_buffer.LogicalPosition < _maxPosition)
            {
                long skipAmount = _maxPosition - _buffer.LogicalPosition;
                while (skipAmount > 0)
                {
                    if (_buffer.AvailableToProcess == 0)
                    {
                        int count = _source.Read(_readBlockBuffer, 0, _readBlockBuffer.Length);
                        if (count == 0)
                        {
                            throw new InvalidOperationException();
                        }

                        _buffer.Put(_readBlockBuffer, 0, count);
                    }
                    
                    int skipStep = (int)Math.Min(skipAmount, _buffer.AvailableToProcess);
                    _buffer.Skip(skipStep);
                    skipAmount -= skipStep;
                }
                return false;
            }
            if (_buffer.LogicalPosition > _maxPosition)
            {
                throw new InvalidOperationException();
            }

            _maxPosition = _stackOfSections.Pop();
            return true;
        }

        private void Check(int grow)
        {
            if (_buffer.LogicalPosition + grow > _maxPosition)
            {
                throw new InvalidOperationException();
            }

            while (_buffer.AvailableToProcess < grow)
            {
                var count = _source.Read(_readBlockBuffer, 0, _readBlockBuffer.Length);
                if (count > 0)
                {
                    _buffer.Put(_readBlockBuffer, 0, count);
                }
            }

            if (_buffer.AvailableToProcess < grow)
            {
                throw new InvalidOperationException();
            }
        }

        public bool ReadBool()
        {
            Check(1);
            switch (_buffer.Pop())
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
            return _buffer.Pop();
        }

        public char ReadChar()
        {
            Check(2);
            CharToByte block = new CharToByte
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop()
            };

            return block.Value;
        }

        public short ReadShort()
        {
            Check(2);
            ShortToByte block = new ShortToByte
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop()
            };

            return block.Value;
        }

        public int ReadInt()
        {
            Check(4);
            IntToByte block = new IntToByte
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop(),
                Byte2 = _buffer.Pop(),
                Byte3 = _buffer.Pop()
            };

            return block.Value;
        }

        public long ReadLong()
        {
            Check(8);
            LongToByte block = new LongToByte()
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop(),
                Byte2 = _buffer.Pop(),
                Byte3 = _buffer.Pop(),
                Byte4 = _buffer.Pop(),
                Byte5 = _buffer.Pop(),
                Byte6 = _buffer.Pop(),
                Byte7 = _buffer.Pop()
            };

            return block.Value;
        }

        public float ReadFloat()
        {
            Check(4);
            FloatToByte block = new FloatToByte()
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop(),
                Byte2 = _buffer.Pop(),
                Byte3 = _buffer.Pop(),
            };

            return block.Value;
        }

        public double ReadDouble()
        {
            Check(8);
            DoubleToByte block = new DoubleToByte()
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop(),
                Byte2 = _buffer.Pop(),
                Byte3 = _buffer.Pop(),
                Byte4 = _buffer.Pop(),
                Byte5 = _buffer.Pop(),
                Byte6 = _buffer.Pop(),
                Byte7 = _buffer.Pop(),
            };

            return block.Value;
        }

        public decimal ReadDecimal()
        {
            Check(16);
            DecimalToByte block = new DecimalToByte()
            {
                Byte0 = _buffer.Pop(),
                Byte1 = _buffer.Pop(),
                Byte2 = _buffer.Pop(),
                Byte3 = _buffer.Pop(),
                Byte4 = _buffer.Pop(),
                Byte5 = _buffer.Pop(),
                Byte6 = _buffer.Pop(),
                Byte7 = _buffer.Pop(),
                Byte8 = _buffer.Pop(),
                Byte9 = _buffer.Pop(),
                Byte10 = _buffer.Pop(),
                Byte11 = _buffer.Pop(),
                Byte12 = _buffer.Pop(),
                Byte13 = _buffer.Pop(),
                Byte14 = _buffer.Pop(),
                Byte15 = _buffer.Pop(),
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

            var bytes = _buffer.Pop(count);
            return Encoding.UTF8.GetString(bytes, 0, count);
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

            return _buffer.Pop(count);
        }
    }
}