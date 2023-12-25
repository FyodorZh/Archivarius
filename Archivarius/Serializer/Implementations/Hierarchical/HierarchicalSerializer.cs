using System;
using System.Collections.Generic;
using System.Reflection;

namespace Archivarius
{
    public class HierarchicalSerializer : PrimitiveSerializer, ISerializer
    {
        private readonly ITypeSerializer _typeSerializer;

        private readonly Dictionary<Type, short> _typeMap = new Dictionary<Type, short>();
        private short _nextDynamicTypeId;

        private readonly Stack<byte> _versionStack = new Stack<byte>();
        private byte _version;

        private readonly ISerializerExtensionsFactory _factory;

        public byte Version => _version;

        public HierarchicalSerializer(
            IWriter writer, 
            ITypeSerializer typeSerializer, 
            ISerializerExtensionsFactory? factory = null,
            int defaultTypeSetVersion = 0, 
            IReadOnlyList<Type>? defaultTypeSet = null)
            : base(writer)
        {
            factory ??= SerializerExtensionsEmptyFactory.Instance;
            factory.OnError += (type, exception) =>
            {
                // TODO
                //OnException?.Invoke(exception);
            };
            _factory = factory;

            _typeSerializer = typeSerializer;
            Prepare(defaultTypeSetVersion, defaultTypeSet);
        }
        
        public void Prepare(int defaultTypeSetVersion = 0, IReadOnlyList<Type>? defaultTypeSet = null)
        {
            if (defaultTypeSetVersion < 0)
            {
                throw new InvalidOperationException(nameof(defaultTypeSetVersion) + " can't be negative");
            }
            
            _typeMap.Clear();
            _nextDynamicTypeId = 1;
            if (defaultTypeSet != null)
            {
                for (int i = 0; i < defaultTypeSet.Count; ++i)
                {
                    _typeMap.Add(defaultTypeSet[i], (short)(-i - 1));
                }
            }
            
            _writer.WriteByte(1); // Protocol type Id == 1
            _writer.WriteInt(defaultTypeSet != null ? defaultTypeSetVersion : -1);
            _writer.WriteByte(0); // Protocol internal version
            _version = 0;
            
            _versionStack.Clear();
        }

        public void AddStruct<T>(ref T value)
            where T : struct, IDataStruct
        {
            value.Serialize(this);
        }

        public void AddVersionedStruct<T>(ref T value)
            where T : struct, IDataStruct, IVersionedData
        {
            _versionStack.Push(_version);
            _version = value.Version;

            _writer.WriteByte(_version);
            value.Serialize(this);

            _version = _versionStack.Pop();
        }

        public void AddClass<T>(ref T? value)
            where T : class, IDataStruct
        {
            if (value == null)
            {
                _writer.WriteShort(0);
            }
            else
            {
                SerializeType(value.GetType());

                _writer.BeginSection();
                SerializeClass(value);
                _writer.EndSection();
            }
        }

        public void AddDynamic<T>(ref T value)
        {
            var extension = _factory.Construct<T>();
            if (extension == null)
                throw new InvalidOperationException($"{typeof(T)} must be recognizable by extensions factory");
            extension.Add(this, ref value);
        }

        protected virtual void SerializeClass(IDataStruct value)
        {
            if (value is IVersionedData versionedData)
            {
                _versionStack.Push(_version);
                _version = versionedData.Version;

                _writer.WriteByte(_version);
                value.Serialize(this);

                _version = _versionStack.Pop();
            }
            else
            {
                value.Serialize(this);
            }
        }

        private void SerializeType(Type type)
        {
            if (!_typeMap.TryGetValue(type, out short typeId))
            {
                typeId = checked(_nextDynamicTypeId++);

                if (null == type.GetConstructor(
                        BindingFlags.CreateInstance |
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null))
                {
                    throw new InvalidOperationException("Type '" + type + "' must have default constructor (public or non-public)");
                }

                _typeMap.Add(type, typeId);
                _writer.WriteShort(typeId);
                _typeSerializer.Serialize(_writer, type);
            }
            else
            {
                _writer.WriteShort(typeId);
            }
        }
    }
}