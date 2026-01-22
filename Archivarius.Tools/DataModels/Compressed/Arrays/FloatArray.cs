using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Archivarius.DataModels.Compressed
{
    [Guid("9F3D9D40-7C9D-4B52-892A-1C7AFF282A39")]
    public class FloatArray : IVersionedDataStruct
    {
        private SerializationScheme _scheme = SerializationScheme.Plain;
        private float[] _values = [];

        public float[] ValuesUnsafeToModify => _values;

        public FloatArray()
        {
        }

        public FloatArray(IEnumerable<float> data)
        {
            _values = data.ToArray();
            _scheme = SerializationScheme.Plain;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _scheme);
            switch (_scheme)
            {
                case SerializationScheme.Plain:
                    Serialize_Plain(serializer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public byte Version => 0;

        private void Serialize_Plain(ISerializer serializer)
        {
            serializer.AddArray(ref _values, () => throw new Exception());
        }
        
        private enum SerializationScheme : byte
        {
            Plain = 0,
        }
    }
}