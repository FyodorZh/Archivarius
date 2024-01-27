using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class ClassEmpty : IDataStruct, IEquatable<ClassEmpty>
    {
        public void Serialize(ISerializer serializer)
        {
        }

        public bool Equals(ClassEmpty? other)
        {
            return true;
        }
    }
    
    public class ClassEmptyV : IVersionedDataStruct, IEquatable<ClassEmptyV>
    {
        public void Serialize(ISerializer serializer)
        {
            Assert.That(serializer.Version, Is.EqualTo(Version));
        }

        public bool Equals(ClassEmptyV? other)
        {
            return true;
        }

        public byte Version => 7;
    }
}