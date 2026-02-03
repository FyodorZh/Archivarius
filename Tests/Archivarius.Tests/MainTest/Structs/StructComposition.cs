using System;

namespace Archivarius.Tests
{
    public struct StructComposition : IDataStruct, IEquatable<StructComposition>
    {
        private int _int;
        
        private StructEmpty _structEmpty;
        private StructEmptyV _structEmptyV;
        private ClassEmpty? _classEmpty;
        private ClassEmptyV? _classEmptyV;

        private StructInt _structInt;
        private ClassInt? _classInt;
        private StructIntV _structIntV;
        private ClassIntV? _classIntV;

        public StructComposition(int p1, int p2, int p3, int p4, int p5)
        {
            _int = p1;
            _structEmpty = new StructEmpty();
            _structEmptyV = new StructEmptyV();
            _classEmpty = new ClassEmpty();
            _classEmptyV = new ClassEmptyV();
            
            _structInt = new StructInt(p2);
            _classInt = new ClassInt(p3);
            _structIntV = new StructIntV(p4);
            _classIntV = new ClassIntV(p5);
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Add(ref _int);
            serializer.AddStruct(ref _structEmpty);
            serializer.AddVersionedStruct(ref _structEmptyV);
            serializer.AddClass(ref _classEmpty);
            serializer.AddClass(ref _classEmptyV);
            serializer.AddStruct(ref _structInt);
            serializer.AddClass(ref _classInt);
            serializer.AddVersionedStruct(ref _structIntV);
            serializer.AddClass(ref _classIntV);
        }

        public bool Equals(StructComposition other)
        {
            return _int.Equals(other._int) &&
                   _structEmpty.Equals(other._structEmpty) &&
                   _structEmptyV.Equals(other._structEmptyV) &&
                   _classEmpty!.Equals(other._classEmpty) &&
                   _classEmptyV!.Equals(other._classEmptyV) &&
                   _structInt.Equals(other._structInt) &&
                   _classInt!.Equals(other._classInt) &&
                   _structIntV.Equals(other._structIntV) &&
                   _classIntV!.Equals(other._classIntV);
        }
    }
}