using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public struct StructEmpty : IDataStruct, IEquatable<StructEmpty>
    {
        public void Serialize(ISerializer serializer)
        {
        }

        public bool Equals(StructEmpty other)
        {
            return true;
        }
    }
    
    public struct StructEmptyV : IVersionedDataStruct, IEquatable<StructEmpty>
    {
        public void Serialize(ISerializer serializer)
        {
            Assert.That(serializer.Version, Is.EqualTo(Version));
        }

        public bool Equals(StructEmpty other)
        {
            return true;
        }

        public byte Version => 7;
    }
}