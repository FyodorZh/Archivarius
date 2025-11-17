using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.UnionDataListBackend
{
    public class UnionDataListReader : IReader
    {
        private readonly List<UnionData> _data;
        private readonly Stack<int> _sectionEnds = new Stack<int>();
        
        private int _maxPosition;
        private int _position;

        public UnionDataListReader(List<UnionData> data)
        {
            _data = data;
            _maxPosition = _data.Count;
            _position = 0;
        }

        public void Reset()
        {
            _position = 0;
            _maxPosition = _data.Count;
            _sectionEnds.Clear();
        }

        private void CheckType(UnionData r, RecordType type)
        {
            if (r._type != type)
            {
                throw new InvalidOperationException();
            }
        }

        public ValueTask<bool> Preload()
        {
            bool hasData = _position < _data.Count;
            return new ValueTask<bool>(hasData);
        }

        public bool TrySetSectionUsage(bool useSections)
        {
            if (useSections)
            {
                return true;
            }

            return false;
        }

        public void BeginSection()
        {
            UnionData r = _data[_position];
            CheckType(r, RecordType.Int);
            int sectionLength = r._value.IntValue;
            
            _sectionEnds.Push(_maxPosition);
            _position += 1;
            _maxPosition = _position + sectionLength;
        }

        public bool EndSection()
        {
            bool isOk = _position == _maxPosition;
            _position = _maxPosition;
            _maxPosition = _sectionEnds.Pop();
            return isOk;
        }

        public bool ReadBool()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Bool);
            return r._value.BoolValue;
        }

        public byte ReadByte()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Byte);
            return r._value.ByteValue;
        }

        public char ReadChar()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Char);
            return r._value.CharValue;
        }

        public short ReadShort()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Short);
            return r._value.ShortValue;
        }

        public int ReadInt()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Int);
            return r._value.IntValue;
        }

        public long ReadLong()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Long);
            return r._value.LongValue;
        }

        public float ReadFloat()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Float);
            return r._value.FloatValue;
        }

        public double ReadDouble()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Double);
            return r._value.DoubleValue;
        }

        public decimal ReadDecimal()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Decimal);
            return r._value.DecimalValue;
        }

        public string? ReadString()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.String);
            return r.Text;
        }
        
        public byte[]? ReadBytes()
        {
            UnionData r = _data[_position++];
            CheckType(r, RecordType.Array);
            return r.Bytes;
        }
    }
}