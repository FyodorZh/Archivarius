using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Archivarius.TypeSerializers
{
    public class GuidBasedTypeDeserializer : ITypeDeserializer
    {
        private static readonly Type _dataStructType = typeof(IDataStruct);

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        [ThreadStatic]
        private static List<string>? _ids;

        public GuidBasedTypeDeserializer()
        {
        }

        public GuidBasedTypeDeserializer(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssembly(assembly);
            }
        }

        public void RegisterAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if ((type is { IsClass: true, IsAbstract: false } || type.IsValueType) // If struct has [Guid] we need to store it too. Because it can be gerneric parameter 
                    && _dataStructType.IsAssignableFrom(type))
                {
                    if (Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false) is GuidAttribute attribute)
                    {
                        if (Guid.TryParse(attribute.Value, out _))
                        {
                            _types.Add(attribute.Value, type);
                        }
                        else
                        {
                            // TODO: throw ERRROR?
                            // TOD: warning channel && errors channel
                        }
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

            var attribute = Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false) as GuidAttribute;
            if (attribute == null || !Guid.TryParse(attribute.Value, out _))
            {
                throw new InvalidOperationException($"{type} must have valid GUID");
            }

            _types.Add(attribute.Value, type);
        }

        public Type? Deserialize(IReader reader)
        {
            byte version = reader.ReadByte();
            switch (version)
            {
                case 0:
                {
                    _ids ??= new List<string>();
                    _ids.Clear();
                    byte count = reader.ReadByte();
                    for (int i = 0; i < count; ++i)
                    {
                        _ids.Add(reader.ReadString() ?? "");
                    }

                    int pos = 0;
                    return ConstructTypeRecursive(_ids, ref pos);
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        private Type? ConstructTypeRecursive(List<string> ids, ref int pos)
        {
            string? typeId = ids[pos++];
            if (typeId == null)
            {
                return null;
            }

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

        private Type? GetTypeById(string id)
        {
            switch (id)
            {
                case "Boolean":
                    return typeof(Boolean);
                case "Byte":
                    return typeof(Byte);
                case "SByte":
                    return typeof(SByte);
                case "Int16":
                    return typeof(Int16);
                case "UInt16":
                    return typeof(UInt16);
                case "Int32":
                    return typeof(Int32);
                case "UInt32":
                    return typeof(UInt32);
                case "Int64":
                    return typeof(Int64);
                case "UInt64":
                    return typeof(UInt64);
                case "Char":
                    return typeof(Char);
                case "Double":
                    return typeof(Double);
                case "Single":
                    return typeof(Single);
                case "String":
                    return typeof(String);
                default:
                    _types.TryGetValue(id, out var type);
                    return type;
            }
        }
    }
}