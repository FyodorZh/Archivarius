using System;
using System.Collections.Generic;
using System.Reflection;

namespace Archivarius
{
    public class HierarchicalDeserializer : PrimitiveDeserializer, ISerializer
    {
        private readonly ITypeReader _typeReader;

        private readonly Stack<byte> _versions = new ();
        private byte _version;
        
        private readonly ISerializerExtensionsFactory? _factory;

        public event Action<Exception>? OnException;

        public byte Version => _version;
        
        public HierarchicalDeserializer(
            IReader reader, 
            ITypeDeserializer typeDeserializer, 
            ISerializerExtensionsFactory? factory = null,
            Func<int, IReadOnlyList<Type>>? defaultTypeSetProvider = null)
            : base(reader)
        {
            var typeReader = new PolymorphicTypeReader(typeDeserializer);
            typeReader.OnException += e => OnException?.Invoke(e);
            _typeReader = typeReader;
            
            if (factory != null)
            {
                factory.OnError += (type, exception) =>
                {
                    OnException?.Invoke(exception);
                };
                _factory = factory;
            }
            
            Prepare(defaultTypeSetProvider);
        }

        public HierarchicalDeserializer(IReader reader)
            : base(reader)
        {
            var typeReader = new TrivialTypeReader();
            _typeReader = typeReader;
            
            Prepare();
        }
        
        public void Prepare(Func<int, IReadOnlyList<Type>>? defaultTypeSetProvider)
        {
            PrepareHeader();

            if (_typeReader is PolymorphicTypeReader polymorphicTypeReader)
            {
                polymorphicTypeReader.Prepare(_reader, defaultTypeSetProvider);
            }
            else
            {
                throw new NotSupportedException($"Unsupported type reader '{_typeReader.GetType()}");
            }
            
            PostPrepareHeader();
        }
        
        public void Prepare()
        {
            if (_typeReader is PolymorphicTypeReader)
            {
                Prepare(null);
                return;
            }
            
            PrepareHeader();
            if (_typeReader is not TrivialTypeReader typeReader)
            {
                throw new NotSupportedException($"Unsupported type reader '{_typeReader.GetType()}");
            }
            PostPrepareHeader();
        }

        private void PrepareHeader()
        {
            byte protocolTypeId = _reader.ReadByte();
            bool useAntiCorruptionSections;
            switch (protocolTypeId)
            {
                case 1:
                    useAntiCorruptionSections = true;
                    break;
                case 2:
                    useAntiCorruptionSections = _reader.ReadBool();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (!_reader.TrySetSectionUsage(useAntiCorruptionSections))
            {
                throw new InvalidOperationException($"{_reader.GetType()} doesn't support section usage '{useAntiCorruptionSections}'");
            }
        }

        private void PostPrepareHeader()
        {
            byte x = _reader.ReadByte();
            switch (x) // RESERVED
            {
                case 0:
                    // OK
                    break;
                default:
                    throw new InvalidOperationException();
            }
            
            _versions.Clear();
            _version = 0;
        }
        
        public void AddStruct<T>(ref T value)
            where T : struct, IDataStruct
        {
            value.Serialize(this);
        }

        public void AddVersionedStruct<T>(ref T value)
            where T : struct, IDataStruct, IVersionedData
        {
            var version = _reader.ReadByte();
            _versions.Push(_version);
            _version = version;
            value.Serialize(this);
            _version = _versions.Pop();
        }

        public void AddClass<T>(ref T? value)
            where T : class, IDataStruct
        {
            IConstructor? ctor = _typeReader.GetConstructor<T>(_reader);
            if (ctor == null) // NULL
            {
                value = null;
                return;
            }

            _reader.BeginSection();

            value = DeserializeClass<T>(ctor);

            if (!_reader.EndSection())
            {
                OnException?.Invoke(new InvalidOperationException("Failed to deserialize object section. Skip it"));
            }
        }

        public void AddDynamic<T>(ref T value)
        {
            var extension = _factory?.Construct<T>();
            if (extension == null)
                throw new InvalidOperationException($"{typeof(T)} must be recognizable by extensions factory");
            extension.Add(this, ref value);
        }

        protected virtual T? DeserializeClass<T>(IConstructor ctor)
            where T : class, IDataStruct
        {
            var value = ctor.Construct() as T;
            if (value != null)
            {
                DeserializeClass(value);
            }

            return value;
        }

        protected void DeserializeClass<T>(T value)
            where T : class, IDataStruct
        {
            bool hasVersion = value is IVersionedData;
            if (hasVersion)
            {
                byte version = _reader.ReadByte();
                _versions.Push(_version);
                _version = version;
            }

            try
            {
                value.Serialize(this);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }

            if (hasVersion)
            {
                _version = _versions.Pop();
            }
        }

        protected interface IConstructor
        {
            bool IsValid { get; }
            object? Construct();
        }

        private class NullConstructor : IConstructor
        {
            public static readonly IConstructor Instance = new NullConstructor();

            public bool IsValid => false;

            public object? Construct()
            {
                return null;
            }
        }

        private class ReflectionBasedConstructor : IConstructor
        {
            private static readonly object[] VoidObjectList = [];

            private readonly ConstructorInfo? _ctorInfo;

            public bool IsValid => _ctorInfo != null;

            public ReflectionBasedConstructor(Type type)
            {
                _ctorInfo = type.GetConstructor(BindingFlags.CreateInstance |
                                                BindingFlags.Instance |
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);
            }

            public object? Construct()
            {
                return _ctorInfo?.Invoke(VoidObjectList);
            }
        }

        private static class TypeConstructorBuilder
        {
            private class TypeConstructor<T> : IConstructor
                where T : class, new()
            {
                public bool IsValid => true;

                public object Construct()
                {
                    return new T();
                }
            }


            public static IConstructor Build(Type type)
            {
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    Type genericCtor = typeof(TypeConstructor<>);
                    Type typeCtor = genericCtor.MakeGenericType(type);
                    return (IConstructor)typeCtor.GetConstructor(Type.EmptyTypes)!.Invoke([]);
                }

                return new ReflectionBasedConstructor(type);
            }
        }

        private interface ITypeReader
        {
            IConstructor? GetConstructor<T>(IReader reader);
        }

        private class PolymorphicTypeReader : ITypeReader
        {
            private readonly ITypeDeserializer _typeDeserializer;

            private readonly List<IConstructor?> _typeMap = new ();
            private readonly List<IConstructor> _defaultTypeMap = new ();
            
            public event Action<Exception>? OnException;

            public PolymorphicTypeReader(ITypeDeserializer typeDeserializer)
            {
                _typeDeserializer = typeDeserializer;
            }

            public void Prepare(IReader reader, Func<int, IReadOnlyList<Type>>? defaultTypeSetProvider = null)
            {
                int defaultTypeSetVersion = reader.ReadInt();

                _typeMap.Clear();
                _defaultTypeMap.Clear();
            
                if (defaultTypeSetVersion >= 0)
                {
                    if (defaultTypeSetProvider == null)
                    {
                        throw new InvalidOperationException();
                    }
                    var defaultTypeSet = defaultTypeSetProvider(defaultTypeSetVersion);
                    for (var i = 0; i < defaultTypeSet.Count; ++i)
                    {
                        _defaultTypeMap.Add(TypeConstructorBuilder.Build(defaultTypeSet[i]));
                    }
                }
            }

            public IConstructor? GetConstructor<T>(IReader reader)
            {
                int flag = reader.ReadShort();
                if (flag == 0) // NULL
                {
                    return null;
                }
                
                IConstructor ctor;
                if (flag > 0)
                {   // dynamic type
                    int typeId = flag - 1;
                    while (typeId >= _typeMap.Count)
                    {
                        _typeMap.Add(null);
                    }

                    IConstructor? dynamicTypeCtor = _typeMap[typeId];
                    if (dynamicTypeCtor == null)
                    {
                        var type = _typeDeserializer.Deserialize(reader);
                        dynamicTypeCtor = type != null ? TypeConstructorBuilder.Build(type) : NullConstructor.Instance;
                        _typeMap[typeId] = dynamicTypeCtor;

                        if (!dynamicTypeCtor.IsValid)
                        {
                            OnException?.Invoke(new InvalidOperationException($"Failed to construct '{type}'. It must have private or public default constructor"));
                        }
                    }

                    ctor = dynamicTypeCtor;
                }
                else
                {   // default type
                    ctor = _defaultTypeMap[-(flag + 1)];
                }

                return ctor;
            }
        }

        private class TrivialTypeReader : ITypeReader
        {
            private readonly Dictionary<Type, IConstructor> _typeMap = new ();
            
            public IConstructor? GetConstructor<T>(IReader reader)
            {
                switch (reader.ReadByte())
                {
                    case 0:
                        return null;
                    case 1:
                        var type = typeof(T);
                        if (!_typeMap.TryGetValue(type, out var constructor))
                        {
                            constructor = TypeConstructorBuilder.Build(type);
                            _typeMap.Add(type, constructor);
                        }
                        return constructor;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}