using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Archivarius
{
    public class HierarchicalSerializer : PrimitiveSerializer, ISerializer
    {
        private readonly ITypeWriter _mainTypeWriter;
        private readonly ITypeWriter _staticTypeWriter;

        private readonly Stack<byte> _versionStack = new Stack<byte>();
        private byte _version;

        private readonly ISerializerExtensionsFactory? _factory;

        public byte Version => _version;

        
        /// <summary>
        /// Construct polymorphic serializer 
        /// </summary>
        public HierarchicalSerializer(
            IWriter writer, 
            ITypeSerializer typeSerializer, 
            ISerializerExtensionsFactory? factory = null,
            bool useAntiCorruptionSections = true,
            int defaultTypeSetVersion = 0, 
            IReadOnlyList<Type>? defaultTypeSet = null)
            : base(writer)
        {
            if (factory != null)
            {
                factory.OnError += (type, exception) =>
                {
                    // TODO
                    //OnException?.Invoke(exception);
                };
                _factory = factory;
            }

            _mainTypeWriter = new PolymorphicTypeWriter(typeSerializer);
            _staticTypeWriter = new TrivialTypeWriter();

            Prepare(useAntiCorruptionSections, defaultTypeSetVersion, defaultTypeSet);
        }

        public HierarchicalSerializer(IWriter writer, bool useAntiCorruptionSections = true)
            : base(writer)
        {
            _staticTypeWriter = _mainTypeWriter = new TrivialTypeWriter();
            Prepare(useAntiCorruptionSections);
        }
        
        public void Prepare(bool useAntiCorruptionSections, int defaultTypeSetVersion, IReadOnlyList<Type>? defaultTypeSet)
        {
            if (defaultTypeSetVersion < 0)
            {
                throw new InvalidOperationException(nameof(defaultTypeSetVersion) + " can't be negative");
            }
            if (_mainTypeWriter is not PolymorphicTypeWriter typeWriter)
            {
                throw new InvalidOperationException($"Wrong TypeWriter type '{_mainTypeWriter.GetType()}'");
            }

            PrepareHeader(useAntiCorruptionSections);
            typeWriter.Prepare(_writer, defaultTypeSetVersion, defaultTypeSet);
            PostPrepareHeader();
        }

        public void Prepare(bool useAntiCorruptionSections = true)
        {
            if (_mainTypeWriter is PolymorphicTypeWriter)
            {
                Prepare(useAntiCorruptionSections, 0, null);
                return;
            }
            
            if (_mainTypeWriter is not TrivialTypeWriter typeWriter)
            {
                throw new InvalidOperationException($"Wrong TypeWriter type '{_mainTypeWriter.GetType()}'");
            }

            PrepareHeader(useAntiCorruptionSections);
            PostPrepareHeader();
        }
        
        private void PrepareHeader(bool useAntiCorruptionSections)
        {
            if (!_writer.TrySetSectionUsage(useAntiCorruptionSections))
            {
                throw new InvalidOperationException($"Writer doesn't support anti corruption sections option '{useAntiCorruptionSections}'");
            }
            
            //byte protocolTypeId = 1; // First protocol (versions till 0.1.0-dev6)
            byte protocolTypeId = 2;  // Added 'useAntiCorruptionSections' option
            
            _writer.WriteByte(protocolTypeId);
            _writer.WriteBool(useAntiCorruptionSections);
        }

        private void PostPrepareHeader()
        {
            _writer.WriteByte(0); // RESERVED
            _version = 0;
            
            _versionStack.Clear();
        }

        public async ValueTask<bool> FlushPoint()
        {
            await _writer.Flush();
            return true;
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
        
        public void AddStaticClass<T>(ref T? value)
            where T : class, IDataStruct
        {
            _staticTypeWriter.WriteType(_writer, ref value);
            if (value != null)
            {
                _writer.BeginSection();
                SerializeClass(value);
                _writer.EndSection();
            }
        }

        public void AddClass<T>(ref T? value)
            where T : class, IDataStruct
        {
            _mainTypeWriter.WriteType(_writer, ref value);
            if (value != null)
            {
                _writer.BeginSection();
                SerializeClass(value);
                _writer.EndSection();
            }
        }
        
        public void AddDynamic<T>(ref T value)
        {
            var extension = _factory?.Construct<T>();
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
        
        private interface ITypeWriter
        {
            void WriteType<T>(IWriter writer, ref T? value);
        }

        private class PolymorphicTypeWriter : ITypeWriter
        {
            private readonly ITypeSerializer _typeSerializer;

            private readonly Dictionary<Type, short> _typeMap = new ();
            private short _nextDynamicTypeId;

            public PolymorphicTypeWriter(ITypeSerializer typeSerializer)
            {
                _typeSerializer = typeSerializer;
            }

            public void Prepare(IWriter writer, int defaultTypeSetVersion, IReadOnlyList<Type>? defaultTypeSet)
            {
                _typeMap.Clear();
                _nextDynamicTypeId = 1;
                if (defaultTypeSet != null)
                {
                    for (int i = 0; i < defaultTypeSet.Count; ++i)
                    {
                        _typeMap.Add(defaultTypeSet[i], (short)(-i - 1));
                    }
                }
                
                writer.WriteInt(defaultTypeSet != null ? defaultTypeSetVersion : -1);
            }
            
            public void WriteType<T>(IWriter writer, ref T? value)
            {
                if (value == null)
                {
                    writer.WriteShort(0);
                    return;
                }
                
                var type = value.GetType();
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
                    writer.WriteShort(typeId);
                    _typeSerializer.Serialize(writer, type);
                }
                else
                {
                    writer.WriteShort(typeId);
                }
            }
        }

        private class TrivialTypeWriter : ITypeWriter
        {
            public void WriteType<T>(IWriter writer, ref T? value)
            {
                if (value == null)
                {
                    writer.WriteByte(0);
                }
                else
                {
                    if (value.GetType() != typeof(T))
                    {
                        throw new InvalidOperationException($"Polymorphic serialization used. {value.GetType()} over {typeof(T)}");
                    }
                    writer.WriteByte(1);
                }
            }
        }
    }
}