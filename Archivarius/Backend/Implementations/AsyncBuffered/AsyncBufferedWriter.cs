using System;
using System.Text;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using Archivarius.Internals;

namespace Archivarius.AsyncBackend
{
    public class AsyncBufferedWriter : IWriter
    {
        private readonly ByteStream _buffer;

        private readonly Func<IByteQueueReader, ValueTask> _asyncWriter;

        public AsyncBufferedWriter(Func<IByteQueueReader, ValueTask> asyncWriter)
        {
            _asyncWriter = asyncWriter;
            _buffer = new ByteStream(128);
            Clear();
        }

        public void Clear()
        {
            _buffer.Clear();
            _buffer.Put(0);
            _buffer.Put(0);
            _buffer.Put(0);
            _buffer.Put(0);
        }

        public async ValueTask Flush()
        {
            int blockSize = (int)_buffer.AvailableToProcess - 4;
            IntToByte block = new IntToByte() { Value = blockSize };
            _buffer.Set(0, block.Byte0);
            _buffer.Set(1, block.Byte1);
            _buffer.Set(2, block.Byte2);
            _buffer.Set(3, block.Byte3);
            await _asyncWriter(_buffer);

            _buffer.Put(0);
            _buffer.Put(0);
            _buffer.Put(0);
            _buffer.Put(0);
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
            _buffer.Put(value ? (byte)1 : (byte)0);
        }
        
        public void WriteByte(byte value)
        {
            _buffer.Put(value);
        }

        public void WriteChar(char value)
        {
            CharToByte block = new CharToByte() {Value = value};
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
        }

        public void WriteShort(short value)
        {
            ShortToByte block = new ShortToByte() {Value = value};
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
        }

        public void WriteInt(int value)
        {
            IntToByte block = new IntToByte() {Value = value};
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
            _buffer.Put(block.Byte2);
            _buffer.Put(block.Byte3);
        }

        public void WriteLong(long value)
        {
            LongToByte block = new LongToByte() {Value = value};
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
            _buffer.Put(block.Byte2);
            _buffer.Put(block.Byte3);
            _buffer.Put(block.Byte4);
            _buffer.Put(block.Byte5);
            _buffer.Put(block.Byte6);
            _buffer.Put(block.Byte7);
        }

        public void WriteFloat(float value)
        {
            FloatToByte block = new FloatToByte() { Value = value };
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
            _buffer.Put(block.Byte2);
            _buffer.Put(block.Byte3);
        }

        public void WriteDouble(double value)
        {
            DoubleToByte block = new DoubleToByte() { Value = value };
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
            _buffer.Put(block.Byte2);
            _buffer.Put(block.Byte3);
            _buffer.Put(block.Byte4);
            _buffer.Put(block.Byte5);
            _buffer.Put(block.Byte6);
            _buffer.Put(block.Byte7);
        }

        public void WriteDecimal(decimal value)
        {
            DecimalToByte block = new DecimalToByte() { Value = value };
            _buffer.Put(block.Byte0);
            _buffer.Put(block.Byte1);
            _buffer.Put(block.Byte2);
            _buffer.Put(block.Byte3);
            _buffer.Put(block.Byte4);
            _buffer.Put(block.Byte5);
            _buffer.Put(block.Byte6);
            _buffer.Put(block.Byte7);
            _buffer.Put(block.Byte8);
            _buffer.Put(block.Byte9);
            _buffer.Put(block.Byte10);
            _buffer.Put(block.Byte11);
            _buffer.Put(block.Byte12);
            _buffer.Put(block.Byte13);
            _buffer.Put(block.Byte14);
            _buffer.Put(block.Byte15);
        }

        public void WriteString(string? value)
        {
            if (value == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _buffer.Put(map.Byte0);
                _buffer.Put(map.Byte1);
                _buffer.Put(map.Byte2);
                _buffer.Put(map.Byte3);
            }
            else
            {
                byte[] utfBytes = Encoding.UTF8.GetBytes(value);
                int count = utfBytes.Length;

                IntToByte map = new IntToByte() { Value = count + 1 };
                _buffer.Put(map.Byte0);
                _buffer.Put(map.Byte1);
                _buffer.Put(map.Byte2);
                _buffer.Put(map.Byte3);

                _buffer.Put(utfBytes, 0, count);
            }
        }

        public void WriteArray(byte[]? bytes)
        {
            if (bytes == null)
            {
                IntToByte map = new IntToByte() {Value = 0};
                _buffer.Put(map.Byte0);
                _buffer.Put(map.Byte1);
                _buffer.Put(map.Byte2);
                _buffer.Put(map.Byte3);
            }
            else
            {
                int count = bytes.Length;

                IntToByte map = new IntToByte() {Value = count + 1};
                _buffer.Put(map.Byte0);
                _buffer.Put(map.Byte1);
                _buffer.Put(map.Byte2);
                _buffer.Put(map.Byte3);
                
                _buffer.Put(bytes, 0, count);
            }
        }

        public void WriteBytes(byte[] value, int offset, int count)
        {
            _buffer.Put(value, offset, count);
        }
    }
}