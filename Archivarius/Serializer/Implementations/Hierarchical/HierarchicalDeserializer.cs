using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archivarius.Constructors;

namespace Archivarius
{
    public class HierarchicalDeserializer : PrimitiveDeserializer, ISerializer
    {
        private readonly ITypeReader _typeReader;

        private readonly Stack<byte> _versions = new ();
        private byte _version;
        
        private readonly IConstructorFactory _constructorFactory;
        
        private readonly ISerializerExtensionsFactory? _factory;

        public event Action<Exception>? OnException;

        public byte Version => _version;

        public static IHierarchicalDeserializer_Params From(IReader source)
        {
            return new HierarchicalDeserializer_Params(source);
        }

        internal HierarchicalDeserializer(IHierarchicalDeserializer_ParamsView @params)
            :base(@params.Source)
        {
            _constructorFactory = @params.ObjectConstructorFactory ?? new DefaultCtorTypeConstructorFactory();
            if (@params.AsPolymorphic() is {} polymorphicParams)
            {
                var typeReader = new PolymorphicTypeReader(
                    polymorphicParams.TypeDeserializer ?? throw new NullReferenceException(),
                    _constructorFactory);
                typeReader.OnException += e => OnException?.Invoke(e);
                _typeReader = typeReader;
            
                if (polymorphicParams.ExtensionsFactory != null)
                {
                    _factory = polymorphicParams.ExtensionsFactory;
                    _factory.OnError += (type, exception) =>
                    {
                        OnException?.Invoke(exception);
                    };
                }

                if (polymorphicParams.AutoPrepare)
                {
                    Prepare(polymorphicParams.DefaultTypeSetProvider);
                }
            }
            else if (@params.AsMonomorphic() is { } monomorphicParams)
            {
                var typeReader = new TrivialTypeReader(_constructorFactory);
                _typeReader = typeReader;

                if (monomorphicParams.AutoPrepare)
                {
                    Prepare();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        
        protected HierarchicalDeserializer(
            IReader reader, 
            ITypeDeserializer typeDeserializer, 
            ISerializerExtensionsFactory? factory = null,
            Func<int, IReadOnlyList<Type>>? defaultTypeSetProvider = null,
            bool autoPrepare = true)
            : base(reader)
        {
            _constructorFactory = new DefaultCtorTypeConstructorFactory();
            var typeReader = new PolymorphicTypeReader(typeDeserializer, _constructorFactory);
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

            if (autoPrepare)
            {
                Prepare(defaultTypeSetProvider);
            }
        }

        protected HierarchicalDeserializer(IReader reader, bool autoPrepare = true)
            : base(reader)
        {
            _constructorFactory = new DefaultCtorTypeConstructorFactory();
            var typeReader = new TrivialTypeReader(_constructorFactory);
            _typeReader = typeReader;

            if (autoPrepare)
            {
                Prepare();
            }
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
            if (_typeReader is not TrivialTypeReader _)
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

        public ValueTask<bool> FlushPoint()
        {
            return _reader.Preload();
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

        private interface ITypeReader
        {
            IConstructor? GetConstructor<T>(IReader reader);
        }

        private class PolymorphicTypeReader : ITypeReader
        {
            private readonly ITypeDeserializer _typeDeserializer;
            private readonly IConstructorFactory _constructorFactory;

            private readonly List<IConstructor?> _typeMap = new ();
            private readonly List<IConstructor> _defaultTypeMap = new ();
            
            public event Action<Exception>? OnException;

            public PolymorphicTypeReader(ITypeDeserializer typeDeserializer, IConstructorFactory constructorFactory)
            {
                _typeDeserializer = typeDeserializer;
                _constructorFactory = constructorFactory;
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
                    foreach (var t in defaultTypeSet)
                    {
                        _defaultTypeMap.Add(_constructorFactory.Build(t));
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
                        dynamicTypeCtor = type != null ? _constructorFactory.Build(type) : NullConstructor.Instance;
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

            private readonly IConstructorFactory _constructorFactory;

            public TrivialTypeReader(IConstructorFactory constructorFactory)
            {
                _constructorFactory = constructorFactory;
            }
            
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
                            constructor = _constructorFactory.Build(type);
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