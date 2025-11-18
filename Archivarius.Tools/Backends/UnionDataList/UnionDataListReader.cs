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

        private void CheckType(UnionData r, UnionDataType type)
        {
            if (r.Type != type)
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
            CheckType(r, UnionDataType.Int);
            int sectionLength = r.Alias.IntValue;
            
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
            CheckType(r, UnionDataType.Bool);
            return r.Alias.BoolValue;
        }

        public byte ReadByte()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Byte);
            return r.Alias.ByteValue;
        }

        public char ReadChar()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Char);
            return r.Alias.CharValue;
        }

        public short ReadShort()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Short);
            return r.Alias.ShortValue;
        }

        public int ReadInt()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Int);
            return r.Alias.IntValue;
        }

        public long ReadLong()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Long);
            return r.Alias.LongValue;
        }

        public float ReadFloat()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Float);
            return r.Alias.FloatValue;
        }

        public double ReadDouble()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Double);
            return r.Alias.DoubleValue;
        }

        public decimal ReadDecimal()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Decimal);
            return r.Alias.DecimalValue;
        }

        public string? ReadString()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.String);
            return r.Text;
        }
        
        public byte[]? ReadBytes()
        {
            UnionData r = _data[_position++];
            CheckType(r, UnionDataType.Array);
            return r.Bytes;
        }
    }
}