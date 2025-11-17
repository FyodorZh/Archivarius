using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.UnionDataListBackend
{
    public class UnionDataListWriter : IWriter
    {
        private List<UnionData> _data = new List<UnionData>();
        private readonly Stack<int> _sectionHeaders = new();
        
        public IReadOnlyList<UnionData> Data => _data;
        
        public List<UnionData> CopyData()
        {
            if (_sectionHeaders.Count > 0)
            {
                throw new InvalidOperationException();
            }

            return new List<UnionData>(_data);
        }

        public List<UnionData> TakeData()
        {
            if (_sectionHeaders.Count > 0)
            {
                throw new InvalidOperationException();
            }

            var res = _data;
            _data = new List<UnionData>();
            return res;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public ValueTask Flush()
        {
            return default;
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
            _sectionHeaders.Push(_data.Count);
            _data.Add(new UnionData(-1));
        }

        public void EndSection()
        {
            int sectionStart = _sectionHeaders.Pop();
            int sectionEnd = _data.Count - 1;
            int sectionLength = sectionEnd - sectionStart;

            _data[sectionStart] = new UnionData(sectionLength);
        }

        public void WriteBool(bool value)
        {
            _data.Add(new UnionData(value));
        }
        
        public void WriteByte(byte value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteChar(char value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteShort(short value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteInt(int value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteLong(long value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteFloat(float value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteDouble(double value)
        {
            _data.Add(new UnionData(value));
        }
        
        public void WriteDecimal(decimal value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteString(string? value)
        {
            _data.Add(new UnionData(value));
        }

        public void WriteBytes(byte[]? value)
        {
            _data.Add(new UnionData(value));
        }
    }
}