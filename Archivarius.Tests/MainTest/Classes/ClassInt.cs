using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class ClassInt : IDataStruct, IEquatable<ClassInt>
    {
        private int _value;
        
        public ClassInt(){}

        public ClassInt(int value)
        {
            _value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _value);
        }

        public bool Equals(ClassInt? other)
        {
            return _value == other!._value;
        }
    }
    
    public class ClassIntV : IVersionedDataStruct, IEquatable<ClassIntV>
    {
        private int _value;
        
        public ClassIntV(){}

        public ClassIntV(int value)
        {
            _value = value;
        }

        public void Serialize(ISerializer serializer)
        {
            Assert.That(serializer.Version, Is.EqualTo(Version));
            serializer.Add(ref _value);
        }

        public bool Equals(ClassIntV? other)
        {
            return _value == other!._value;
        }

        public byte Version => 123;
    }
}