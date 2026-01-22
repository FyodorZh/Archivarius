using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Archivarius.DataModels.Compressed
{
    [Guid("9CBBD500-A793-43CB-8429-2C88721A17A1")]
    public class LongArray : IVersionedDataStruct 
    {
        private SerializationScheme _scheme = SerializationScheme.Plain;
        private long[] _values = [];
        
        public long[] ValuesUnsafeToModify => _values;
        
        public LongArray()
        {
        }
        
        public LongArray(IEnumerable<long> data)
        {
            _values = data.ToArray();

            long delta = 0;
            for (int i = 1; i < _values.Length; i++)
            {
                delta = Math.Max(delta, _values[i] - _values[i - 1]);
            }

            if (delta >= SByte.MinValue && delta <= SByte.MaxValue)
            {
                _scheme = SerializationScheme.SByteDelta;
            }
            else if (delta >= Int16.MinValue && delta <= Int16.MaxValue)
            {
                _scheme = SerializationScheme.ShortDelta;
            }
            else if (delta >= Int32.MinValue && delta <= Int32.MaxValue)
            {
                _scheme = SerializationScheme.IntDelta;
            }
            else
            {
                _scheme = SerializationScheme.Plain;
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
                case SerializationScheme.IntDelta:
                case SerializationScheme.ShortDelta:
                case SerializationScheme.SByteDelta:
                    Serialize_Delta(serializer, _scheme);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public byte Version => 0;

        private void Serialize_Plain(ISerializer serializer)
        {
            serializer.AddArray(ref _values, () => throw new Exception());
        }
        
        private void Serialize_Delta(ISerializer serializer, SerializationScheme scheme)
        {
            if (serializer.IsWriter)
            {
                var writer = serializer.Writer;
                writer.WriteInt(_values.Length);
                if (_values.Length > 0)
                {
                    writer.WriteLong(_values[0]);
                    switch (scheme)
                    {
                        case SerializationScheme.IntDelta:
                            for (int i = 1; i < _values.Length; i++)
                            {
                                var delta = (int)(_values[i] - _values[i - 1]);
                                writer.WriteInt(delta);
                            }
                            break;
                        case SerializationScheme.ShortDelta:
                            for (int i = 1; i < _values.Length; i++)
                            {
                                var delta = (short)(_values[i] - _values[i - 1]);
                                writer.WriteShort(delta);
                            }
                            break;
                        case SerializationScheme.SByteDelta:
                            for (int i = 1; i < _values.Length; i++)
                            {
                                var delta = (sbyte)(_values[i] - _values[i - 1]);
                                writer.WriteSByte(delta);
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
            else
            {
                var reader = serializer.Reader;
                int len = reader.ReadInt();
                _values = new long[len];
                if (len > 0)
                {
                    _values[0] = reader.ReadLong();
                    switch (scheme)
                    {
                        case SerializationScheme.IntDelta:
                            for (int i = 1; i < len; i++)
                            {
                                var delta = reader.ReadInt();
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        case SerializationScheme.ShortDelta:
                            for (int i = 1; i < len; i++)
                            {
                                var delta = reader.ReadShort();
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        case SerializationScheme.SByteDelta:
                            for (int i = 1; i < len; i++)
                            {
                                var delta = reader.ReadSByte();
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        private static int PredictSize(SerializationScheme scheme, int count)
        {
            switch (scheme)
            {
                case SerializationScheme.Plain:
                    return count * 8;
                case SerializationScheme.IntDelta:
                    return count * 4;
                case SerializationScheme.ShortDelta:
                    return count * 2;
                case SerializationScheme.SByteDelta:
                    return count;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scheme), scheme, null);
            }
        }
        
        private enum SerializationScheme : byte
        {
            Plain = 0,
            IntDelta = 1,
            ShortDelta = 2,
            SByteDelta = 3,
        }
    }
}