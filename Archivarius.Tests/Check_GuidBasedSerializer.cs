using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Archivarius.TypeSerializers;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class Check_GuidBasedSerializer
    {
        [Guid("7AED94EB-F03B-41E7-9379-F260BC03FAD2")]
        private class ClassSimple : IDataStruct
        {
            public int Data;
            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref Data);
            }
        }
        
        [Guid("1E7CF652-CBFE-49C3-B124-C4A9862718A8")]
        private class ClassGeneric1<T> : IDataStruct
            where T : class, IDataStruct
        {
            public T? Data;
            
            public void Serialize(ISerializer serializer)
            {
                serializer.AddClass(ref Data);
            }
        }
        
        [Guid("0B4BD172-2A13-4569-B284-64824C25D7EF")]
        private class ClassGeneric2<T1, T2> : IDataStruct
            where T2 : class, IDataStruct
        {
            public T2? Data2;

            public string T1Name => typeof(T1).Name;
            
            public void Serialize(ISerializer serializer)
            {
                serializer.AddClass(ref Data2);
            }
        }
        
        [Test]
        public void Test()
        {
            ReaderWriterStream stream = new ReaderWriterStream();

            HierarchicalSerializer serializer = new HierarchicalSerializer(stream, new GuidBasedTypeSerializer(), null, false);
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(stream, new GuidBasedTypeDeserializer(
                new[] { typeof(Check_GuidBasedSerializer).Assembly }));

            {
                ClassSimple? simple = new ClassSimple() { Data = 7 };
                serializer.AddClass(ref simple);
                simple = null;
                deserializer.AddClass(ref simple);
                Assert.That(simple!.Data, Is.EqualTo(7));
            }

            {
                ClassSimple simple = new ClassSimple() { Data = 7 };
                ClassGeneric1<ClassSimple>? generic = new ClassGeneric1<ClassSimple>() { Data = simple };
                serializer.AddClass(ref generic);
                generic = null;
                deserializer.AddClass(ref generic);
                Assert.That(generic!.Data!.Data, Is.EqualTo(7));
            }
            
            {
                ClassSimple simple = new ClassSimple() { Data = 123 };
                ClassGeneric2<int, ClassSimple>? generic = new ClassGeneric2<int, ClassSimple>() { Data2 = simple };
                serializer.AddClass(ref generic);
                generic = null;
                deserializer.AddClass(ref generic);
                Assert.That(generic!.T1Name, Is.EqualTo(nameof(Int32)));
                Assert.That(generic!.Data2!.Data, Is.EqualTo(123));
            }
            
            {
                ClassSimple simple = new ClassSimple() { Data = 123 };
                var generic1 = new ClassGeneric1<ClassSimple>() { Data = simple };
                var generic2 = new ClassGeneric2<string, ClassGeneric1<ClassSimple>>() { Data2 = generic1 };
                serializer.AddClass(ref generic2);
                generic2 = null;
                deserializer.AddClass(ref generic2);
                Assert.That(generic2!.T1Name, Is.EqualTo("String"));
                Assert.That(generic2!.Data2!.Data!.Data, Is.EqualTo(123));
            }
        }
    }
}