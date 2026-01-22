using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Archivarius.DataModels.Compressed
{
    [Guid("EC886A88-FBE7-4411-B750-6FFC2E7BA331")]
    public class DoubleArray : IVersionedDataStruct
    {
        private SerializationScheme _scheme = SerializationScheme.Plain;
        private double[] _values = [];

        public double[] ValuesUnsafeToModify => _values;

        public DoubleArray()
        {
        }

        public DoubleArray(IEnumerable<double> data)
        {
            _values = data.ToArray();

            _scheme = SerializationScheme.Plain;
            if (_values.Length > 1)
            {
                double last = _values[0];
                for (int i = 1; i < _values.Length; i++)
                {
                    float delta = (float)(_values[i] - last);
                    last += delta;
                    var eps = Math.Abs(last - _values[i]) / (Math.Abs(_values[i]) + 1);
                    if (eps > 1e-8)
                    {    
                        return;
                    }
                }
                _scheme = SerializationScheme.AsFloatDelta;
            }
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _scheme);
            switch (_scheme)
            {
                case SerializationScheme.Plain:
                    Serialize_Plain(serializer);
                    break;
                case SerializationScheme.AsFloatDelta:
                    Serialize_AsFloat(serializer);
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

        private void Serialize_AsFloat(ISerializer serializer)
        {
            if (serializer.IsWriter)
            {
                var writer = serializer.Writer;
                writer.WriteInt(_values.Length);
                if (_values.Length > 0)
                {
                    writer.WriteDouble(_values[0]);
                    double last = _values[0];
                    for (int i = 1; i < _values.Length; i++)
                    {
                        var delta = (float)(_values[i] - last);
                        writer.WriteFloat(delta);
                        last += delta;
                    }
                }
            }
            else
            {
                var reader = serializer.Reader;
                int len = reader.ReadInt();
                _values = new double[len];
                if (len > 0)
                {
                    _values[0] = reader.ReadDouble();
                    for (int i = 1; i < len; i++)
                    {
                        var delta = reader.ReadFloat();
                        _values[i] = _values[i - 1] + delta;
                    }
                }
            }
        }
        
        private enum SerializationScheme : byte
        {
            Plain = 0,
            AsFloatDelta = 1
        }
    }
}