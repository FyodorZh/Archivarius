using System;
using Archivarius.AsyncBackend;

namespace Archivarius
{
    namespace AsyncBackend
    {
        public interface IByteQueueWriter
        {
            void Put(byte[] values, int start, int count);
        }

        public interface IByteQueueReader
        {
            int Pop(byte[] dst, int start, int count);
        }
    }

    namespace Internals
    {
        public class ByteStream : IByteQueueWriter, IByteQueueReader
        {
            private int _capacity; // Power of 2
            private long _capacityMask;

            private int _count;
            private long _position;

            private byte[] _buffer;

            public ByteStream()
                : this(16)
            {
            }

            public ByteStream(int capacity)
            {
                _capacity = BitMath.NextPow2((uint)capacity);
                _capacityMask = _capacity - 1;

                _count = 0;
                _position = 0;
                _buffer = new byte[_capacity];
            }

            public void Clear()
            {
                _count = 0;
                _position = 0;
            }

            public long LogicalPosition => _position;

            public long AvailableToProcess => _count;

            private void Grow(int delta)
            {
                if (_count + delta > _capacity)
                {
                    int newCapacity = _capacity;
                    while (_count + delta > newCapacity)
                        newCapacity *= 2;
                    long newCapacityMask = newCapacity - 1;

                    byte[] newData = new byte[newCapacity];
                    Buffer.BlockCopy(_buffer, (int)(_position & _capacityMask),
                        newData, (int)(_position & newCapacityMask),
                        Math.Min(_capacity - (int)(_position & _capacityMask), _count));
                    int tail = _count - (_capacity - (int)(_position & _capacityMask));
                    if (tail > 0)
                    {
                        Buffer.BlockCopy(_buffer, 0,
                            newData, (int)(_position & newCapacityMask) + (_count - tail),
                            tail);
                    }

                    _capacity = newCapacity;
                    _capacityMask = newCapacityMask;
                    _buffer = newData;
                }
            }

            public void Set(int id, byte value)
            {
                _buffer[(_position + id) & _capacityMask] = value;
            }

            public void Put(byte value)
            {
                Grow(1);
                _buffer[(_position + (_count++)) & _capacityMask] = value;
            }
            
            public void Put(byte[] values, int start, int count)
            {
                Grow(count);
                for (int i = 0; i < count; ++i)
                {
                    _buffer[(_position + _count + i) & _capacityMask] = values[start + i];
                }

                _count += count;
            }

            public byte Pop()
            {
                if (_count > 0)
                {
                    _count -= 1;
                    return _buffer[(_position++) & _capacityMask];
                }

                throw new IndexOutOfRangeException();
            }
            
            public int Pop(byte[] dst, int start, int count)
            {
                count = Math.Min(count, _count);
                
                int firstPartSize = Math.Min(_capacity - (int)(_position & _capacityMask), count);
                Buffer.BlockCopy(_buffer, (int)(_position & _capacityMask), dst, start, firstPartSize);
                int secondPartSize = count - firstPartSize;
                if (secondPartSize > 0)
                {
                    Buffer.BlockCopy(_buffer, 0, dst, firstPartSize, secondPartSize);
                }

                _position += count;
                _count -= count;
                return count;
            }

            public byte[] Pop(int count)
            {
                if (_count >= count)
                {
                    _count -= count;
                    byte[] result = new byte[count];
                    for (int i = 0; i < count; ++i)
                    {
                        result[i] = _buffer[(_position++) & _capacityMask];
                    }

                    return result;
                }

                throw new IndexOutOfRangeException();
            }
            
            public void PopNBytes(byte[] dst, int offset, int count)
            {
                if (_count >= count)
                {
                    _count -= count;
                    for (int i = 0; i < count; ++i)
                    {
                        dst[offset + i] = _buffer[(_position++) & _capacityMask];
                    }

                    return;
                }

                throw new IndexOutOfRangeException();
            }

            public void Skip(int count)
            {
                if (_count >= count)
                {
                    _count -= count;
                    _position += count;
                }

                throw new IndexOutOfRangeException();
            }

            public byte[] ToArray()
            {
                byte[] result = new byte[_count];
                for (int i = 0; i < _count; ++i)
                {
                    result[i] = _buffer[(_position + i) & _capacityMask];
                }

                return result;
            }
        }
    }
}
