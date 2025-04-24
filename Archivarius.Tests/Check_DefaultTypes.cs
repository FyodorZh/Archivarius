using System;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_DefaultTypes
    {
        [Test]
        public void DoTest()
        {
            ReaderWriterStream stream = new ReaderWriterStream();

            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, new TmpTypeSerializer());
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(stream, new TmpTypeDeserializer());

            var defaultTypes = new Type[]
            {
                typeof(A),
                typeof(B)
            };
            
            serializer.Prepare(true, 3, defaultTypes);
            deserializer.Prepare(version =>
            {
                if (version != 3)
                {
                    throw new Exception();
                }
                return defaultTypes;
            });
            
            {
                A? a = new A() { Data = 7 };
                B? b = new B() { Data = 77, NestedA = new A() { Data = 777 }, NestedC = new C()};
                C? c = new C() { Data = 7777 };

                serializer.AddClass(ref a);
                serializer.AddClass(ref b);
                serializer.AddClass(ref c);
            }

            {
                A? a = null;
                B? b = null;
                C? c = null;
                deserializer.AddClass(ref a);
                deserializer.AddClass(ref b);
                deserializer.AddClass(ref c);
                
                Assert.That(a!.Data, Is.EqualTo(7));
                Assert.That(b!.Data, Is.EqualTo(77));
                Assert.That(b!.NestedA!.Data, Is.EqualTo(777));
                Assert.That(b!.NestedC!.Data, Is.EqualTo(0));
                Assert.That(c!.Data, Is.EqualTo(7777));
            }
        }

        private class TmpTypeSerializer : ITypeSerializer
        {
            public void Serialize(IWriter writer, Type type)
            {
                if (type != typeof(C))
                {
                    throw new InvalidOperationException("Wrong type " + type);
                }
            }
        }

        private class TmpTypeDeserializer : ITypeDeserializer
        {
            public Type? Deserialize(IReader reader)
            {
                return typeof(C);
            }
        }
        
        private class A : IDataStruct
        {
            public int Data;
            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Data);
            }
        }

        private class B : IDataStruct
        {
            public int Data;
            public A? NestedA;
            public C? NestedC;
            
            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Data);
                serializer.AddClass(ref NestedA);
                serializer.AddClass(ref NestedC);
            }
        }
        
        private class C : IDataStruct
        {
            public int Data;
            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Data);
            }
        }
    }
}