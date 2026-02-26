using System;
using System.Runtime.InteropServices;

namespace Archivarius.DataModels.Compressed
{
    [Guid("9CBBD500-A793-43CB-8429-2C88721A17A1")]
    public class LongArray : IVersionedDataStruct 
    {
        private SerializationScheme _scheme = SerializationScheme.Plain;
        private long[] _values = Array.Empty<long>();
        private long _gcd = 1;
        
        public long[] ValuesUnsafeToModify => _values;
        
        public LongArray()
        {
        }
        
        public LongArray(long[] data)
        {
            _values = data;

            if (data.Length <= 2)
            {
                return; // plain
            }

            long minDelta, maxDelta;
            try
            {
                long gcd = maxDelta = minDelta = checked(_values[1] - _values[0]);
                for (int i = 2; i < _values.Length; i++)
                {
                    var d = checked(_values[i] - _values[i - 1]);
                    maxDelta = Math.Max(maxDelta, d);
                    minDelta = Math.Min(minDelta, d);
                    gcd = Gcd(Math.Abs(d), gcd);
                }

                minDelta /= gcd;
                maxDelta /= gcd;
                _gcd = gcd;
            }
            catch
            {
                return; // plain
            }

            if (minDelta >= SByte.MinValue && maxDelta <= SByte.MaxValue)
            {
                _scheme = SerializationScheme.SByteDelta;
            }
            else if (minDelta >= Int16.MinValue && maxDelta <= Int16.MaxValue)
            {
                _scheme = SerializationScheme.ShortDelta;
            }
            else if (minDelta >= Int32.MinValue && maxDelta <= Int32.MaxValue)
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
            
            if (serializer.Version > 0)
            {
                if (_scheme != SerializationScheme.Plain)
                {
                    serializer.Add(ref _gcd);
                }
            }
            
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

        public byte Version => 1;

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
                                var delta = (int)((_values[i] - _values[i - 1]) / _gcd);
                                writer.WriteInt(delta);
                            }
                            break;
                        case SerializationScheme.ShortDelta:
                            for (int i = 1; i < _values.Length; i++)
                            {
                                var delta = (short)((_values[i] - _values[i - 1]) / _gcd);
                                writer.WriteShort(delta);
                            }
                            break;
                        case SerializationScheme.SByteDelta:
                            for (int i = 1; i < _values.Length; i++)
                            {
                                var delta = (sbyte)((_values[i] - _values[i - 1]) / _gcd);
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
                                var delta = reader.ReadInt() * _gcd;
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        case SerializationScheme.ShortDelta:
                            for (int i = 1; i < len; i++)
                            {
                                var delta = reader.ReadShort() * _gcd;
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        case SerializationScheme.SByteDelta:
                            for (int i = 1; i < len; i++)
                            {
                                var delta = reader.ReadSByte() * _gcd;
                                _values[i] = _values[i - 1] + delta;
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        public int GetSizePerRecord() => PredictSize(_scheme, 1);

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
        
        private static long Gcd(long a, long b) // positive
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
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