using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Archivarius.CompressedData
{
    [Guid("0FADA21C-30A5-4B12-90CB-EADA52FCECD2")]
    public class BoolArray : IVersionedDataStruct
    {
        private byte[] _data = [];
        private byte _lastSize;

        private int _count;
        
        public int Count => _count;
        
        public bool this[int pos]
        {
            get => (_data[pos / 8] & (byte)(1 << (pos % 8))) != 0;
            set
            {
                int x = pos / 8;
                if (value)
                {
                    _data[x] |= (byte)(1 << (pos % 8));
                }
                else
                {
                    _data[x] &= (byte)(~(1 << (pos % 8)));
                }
            }
        }
        
        public BoolArray()
        {
        }

        public BoolArray(IReadOnlyList<bool> array)
        {
            _count = array.Count;
            _lastSize = (byte)(_count % 8);
            int len = (_count + 7) / 8;
            _data = new byte[len];
            for (int i = 0; i < _count; i++)
            {
                if (array[i])
                {
                    int pos = i / 8;
                    int id = i % 8;
                    _data[pos] |= (byte)(1 << id);
                }
            }
        }
        
        public bool[] ToArray()
        {
            bool[] array = new bool[_count];
            for (int i = 0; i < _count; i++)
            {
                int pos = i / 8;
                int id = i % 8;
                array[i] = (_data[pos] & (1 << id)) != 0;
            }
            return array;
        }
        
        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _data, () => throw new Exception());
            serializer.Add(ref _lastSize);
            if (!serializer.IsWriter)
            {
                if (_data.Length > 0)
                {
                    _count = _data.Length * 8 - 8 + _lastSize;
                }
                else
                {
                    _count = 0;
                }
            }
        }

        public byte Version => 0;
    }
}