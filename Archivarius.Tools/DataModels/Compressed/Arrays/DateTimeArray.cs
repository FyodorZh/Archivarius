using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Archivarius.DataModels.Compressed;

namespace Archivarius
{
    [Guid("775E3F2E-2071-4015-A6C4-63D51B174D11")]
    public class DateTimeArray : IVersionedDataStruct
    {
        private LongArray _values;

        public int Length => _values.ValuesUnsafeToModify.Length;

        public int GetSizePerRecord() => _values.GetSizePerRecord();
        
        public DateTimeArray()
        {
            _values = new LongArray();
        }
        
        public DateTimeArray(IReadOnlyList<DateTime> times)
        {
            long[] values = new long[times.Count];
            for (int i = 0; i < times.Count; i++)
            {
                values[i] = times[i].ToBinary();
            }
            
            _values = new LongArray(values);
        }
        
        public DateTime this[int id]
        {
            get
            {
                long value = _values.ValuesUnsafeToModify[id];
                return DateTime.FromBinary(value);
            }
        }

        public DateTime[] ToArray()
        {
            DateTime[] array = new DateTime[_values.ValuesUnsafeToModify.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this[i];
            }
            return array;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.AddStaticClass(ref _values, () => throw new Exception());
        }

        public byte Version => 0;
    }
}