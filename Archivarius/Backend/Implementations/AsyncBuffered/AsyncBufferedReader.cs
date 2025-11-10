using System;
using System.Text;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using Archivarius.Internals;

namespace Archivarius.AsyncBackend
{
    public class AsyncBufferedReader : IReader
    {
        private readonly ByteStream _buffer;
        private readonly Func<IByteQueueWriter, int, ValueTask> _readAsync;

        public AsyncBufferedReader(Func<IByteQueueWriter, int, ValueTask> readAsync)
        {
            _buffer = new ByteStream(128);
            _readAsync = readAsync;
        }

        public async ValueTask<bool> Preload()
        {
            if (!await EnsureBufferSize(4))
            {
                return false;
            }
            int currentBlockSize = ReadInt();
            if (currentBlockSize == 0)
            {
                return false;
            }

            if (!await EnsureBufferSize(currentBlockSize))
            {
                return false;
            }

            return true;
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
        }

        public bool EndSection()
        {
            return true;
        }

        private async ValueTask<bool> EnsureBufferSize(int size)
        {
            if (_buffer.AvailableToProcess < size)
            {
                await _readAsync(_buffer, size - (int)_buffer.AvailableToProcess);
                return _buffer.AvailableToProcess >= size;
            }

            return true;
        }

        private void Check(int grow)
        {
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