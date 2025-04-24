using System.Collections.Generic;

namespace Archivarius
{
    public class ReaderWriterStream : IReader, IWriter
    {
        private readonly Queue<bool> _booleans = new Queue<bool>();
        private readonly Queue<byte> _bytes = new Queue<byte>();
        private readonly Queue<char> _chars = new Queue<char>();
        private readonly Queue<short> _shorts = new Queue<short>();
        private readonly Queue<int> _ints = new Queue<int>();
        private readonly Queue<long> _longs = new Queue<long>();
        private readonly Queue<float> _floats = new Queue<float>();
        private readonly Queue<double> _doubles = new Queue<double>();
        private readonly Queue<decimal> _decimals = new Queue<decimal>();
        private readonly Queue<string?> _strings = new Queue<string?>();
        private readonly Queue<byte[]?> _arrays = new Queue<byte[]?>();

        public bool IsEmpty =>
            _booleans.Count == 0 &&
            _bytes.Count == 0 &&
            _chars.Count == 0 &&
            _shorts.Count == 0 &&
            _ints.Count == 0 &&
            _longs.Count == 0 &&
            _floats.Count == 0 &&
            _doubles.Count == 0 &&
            _decimals.Count == 0 &&
            _strings.Count == 0 &&
            _arrays.Count == 0;

        public void Clear()
        {
            _booleans.Clear(); 
            _bytes.Clear();
            _chars.Clear();
            _shorts.Clear();
            _ints.Clear();
            _longs.Clear();
            _floats.Clear();
            _doubles.Clear();
            _decimals.Clear();
            _strings.Clear();
            _arrays.Clear();
        }

        public bool ReadBool()
        {
            return _booleans.Dequeue();
        }

        public byte ReadByte()
        {
            return _bytes.Dequeue();
        }

        public char ReadChar()
        {
            return _chars.Dequeue();
        }

        public short ReadShort()
        {
            return _shorts.Dequeue();
        }

        public int ReadInt()
        {
            return _ints.Dequeue();
        }

        public long ReadLong()
        {
            return _longs.Dequeue();
        }

        public float ReadFloat()
        {
            return _floats.Dequeue();
        }

        public double ReadDouble()
        {
            return _doubles.Dequeue();
        }

        public decimal ReadDecimal()
        {
            return _decimals.Dequeue();
        }

        public string? ReadString()
        {
            return _strings.Dequeue();
        }

        public byte[]? ReadBytes()
        {
            return _arrays.Dequeue();
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            // DO NOTHING
            return true;
        }

        void IWriter.BeginSection()
        {
            // DO NOTHING
        }

        void IWriter.EndSection()
        {
            // DO NOTHING
        }

        void IReader.BeginSection()
        {
            // DO NOTHING
        }

        bool IReader.EndSection()
        {
            return true;
        }

        public void WriteBool(bool value)
        {
            _booleans.Enqueue(value);
        }

        public void WriteByte(byte value)
        {
            _bytes.Enqueue(value);
        }

        public void WriteChar(char value)
        {
            _chars.Enqueue(value);
        }

        public void WriteShort(short value)
        {
            _shorts.Enqueue(value);
        }

        public void WriteInt(int value)
        {
            _ints.Enqueue(value);
        }

        public void WriteLong(long value)
        {
            _longs.Enqueue(value);
        }

        public void WriteFloat(float value)
        {
            _floats.Enqueue(value);
        }

        public void WriteDouble(double value)
        {
            _doubles.Enqueue(value);
        }

        public void WriteDecimal(decimal value)
        {
            _decimals.Enqueue(value);
        }

        public void WriteString(string? value)
        {
            _strings.Enqueue(value);
        }

        public void WriteBytes(byte[]? value)
        {
            _arrays.Enqueue(value);
        }
    }
}