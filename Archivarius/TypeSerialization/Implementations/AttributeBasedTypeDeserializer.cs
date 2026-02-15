using System;
using System.Collections.Generic;
using System.Reflection;

namespace Archivarius.TypeSerializers
{
    public abstract class AttributeBasedTypeDeserializer<TId> : ITypeDeserializer
    {
        private readonly Type _dataStructType = typeof(IDataStruct);

        private readonly Dictionary<TId, Type> _types = new Dictionary<TId, Type>();

        protected abstract List<TId> GetTmpList();
        protected abstract bool GetTypeId(Type type, out TId id);
        protected abstract TId ReadId(IReader reader);
        protected abstract Type? GetSystemTypeById(TId id);

        public AttributeBasedTypeDeserializer<TId> Add(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssembly(assembly);
            }

            return this;
        }

        public AttributeBasedTypeDeserializer<TId> Add(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterType(type);
            }

            return this;
        }

        public void RegisterAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if ((type is { IsClass: true, IsAbstract: false } || type.IsValueType) // If struct has [Attribute] we need to store it too. Because it can be gerneric parameter 
                    && _dataStructType.IsAssignableFrom(type))
                {
                    if (GetTypeId(type, out var id))
                    {
                        _types.Add(id, type);
                    }
                    else
                    {
                        // TODO: throw ERRROR?
                        // TOD: warning channel && errors channel
                    }
                }
            }
        }

        public void RegisterType(Type type)
        {
            if ((type is not { IsClass: true, IsAbstract: false } && !type.IsValueType) || !_dataStructType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type} must inherit from IDataStruct and be not abstract");
            }

            if (GetTypeId(type, out var id))
            {
                _types.Add(id, type);
            }
            else
            {
                throw new InvalidOperationException($"{type} must have valid identification attribute");
            }
        }

        public Type? Deserialize(IReader reader)
        {
            byte version = reader.ReadByte();
            switch (version)
            {
                case 0:
                {
                    var ids = GetTmpList();
                    byte count = reader.ReadByte();
                    for (int i = 0; i < count; ++i)
                    {
                        ids.Add(ReadId(reader));
                    }

                    int pos = 0;
                    return ConstructTypeRecursive(ids, ref pos);
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        private Type? ConstructTypeRecursive(List<TId> ids, ref int pos)
        {
            TId typeId = ids[pos++];

            Type? type = GetTypeById(typeId);
            if (type == null)
            {
                return null;
            }

            if (type.IsGenericType)
            {
                var genericArguments = new Type[type.GetGenericArguments().Length];
                for (int i = 0; i < genericArguments.Length; ++i)
                {
                    var typeArg = ConstructTypeRecursive(ids, ref pos);
                    if (typeArg == null)
                    {
                        return null;
                    }

                    genericArguments[i] = typeArg;
                }
                type = type.MakeGenericType(genericArguments);
            }

            return type;
        }

        private Type? GetTypeById(TId id)
        {
            Type? type = GetSystemTypeById(id);
            if (type == null)
            {
                _types.TryGetValue(id, out type);
            }

            return type;
        }
    }
}