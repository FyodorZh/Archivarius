using System;
using System.Linq;
using System.Runtime.InteropServices;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class Check_TypeSerializer
    {
        private Type[] _userTypes = new Type[]
        {
            typeof(TClass),
            //typeof(TStruct),
            typeof(TStructAttr),
            typeof(Gen<TStructAttr, Gen<TClass, bool>>)
        };
        
        private Type[] _systemTypes = new Type[]
        {
            typeof(bool),
            typeof(byte),
            typeof(char),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
        };
        
        [Test]
        public void Test_TypeNameBased()
        {
            _systemTypes = _systemTypes.Concat(_userTypes).ToArray();
            Test(new TypenameBasedTypeSerializer(), new TypenameBasedTypeDeserializer(), _systemTypes);
        }
        
        [Test]
        public void Test_GuidBased()
        {
            Test(new GuidBasedTypeSerializer(), 
                new GuidBasedTypeDeserializer().Add(typeof(Check_TypeSerializer).Assembly), 
                _userTypes.Concat(_systemTypes).ToArray());
        }
        
        [Test]
        public void Test_IdBased()
        {
            Test(new IdBasedTypeSerializer(), 
                new IdBasedTypeDeserializer().Add(typeof(Check_TypeSerializer).Assembly), 
                _userTypes.Concat(_systemTypes).ToArray());
        }

        private void Test(ITypeSerializer serializer, ITypeDeserializer deserializer, Type[] types)
        {
            ReaderWriterStream rwStream = new ReaderWriterStream();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                serializer.Serialize(rwStream, type);
                var deserializedType = deserializer.Deserialize(rwStream);
                Assert.That(deserializedType, Is.EqualTo(type));
            }
        }

        [Guid("DAFF9C57-40FD-4C35-9C52-245A20FEA72D")]
        [Id(1, 2)]
        private class TClass : IDataStruct
        {
            public void Serialize(ISerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
        
        private struct TStruct : IDataStruct
        {
            public void Serialize(ISerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
        
        [Guid("37CF668F-1745-4B18-B3C5-84F4F23FB8B7")]
        [Id(2,4)]
        private struct TStructAttr : IDataStruct
        {
            public void Serialize(ISerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
        
        [Guid("504297FC-CE87-49A0-B487-CBEEE1DB475B")]
        [Id(4,2)]
        private class Gen<T1, T2> : IDataStruct
        {
            public void Serialize(ISerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}