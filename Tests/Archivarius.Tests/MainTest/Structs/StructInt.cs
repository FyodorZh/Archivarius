using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public struct StructInt : IDataStruct, IEquatable<StructInt>
    {
        private int _value;
        
        public StructInt(){}

        public StructInt(int value)
        {
            _value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _value);
        }

        public bool Equals(StructInt other)
        {
            return _value == other._value;
        }
    }
    
    public struct StructIntV : IVersionedDataStruct, IEquatable<StructIntV>
    {
        private int _value;
        
        public StructIntV(){}

        public StructIntV(int value)
        {
            _value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            Assert.That(serializer.Version, Is.EqualTo(Version));
            serializer.Add(ref _value);
        }

        public bool Equals(StructIntV other)
        {
            return _value == other._value;
        }

        public byte Version => 123;
    }
}