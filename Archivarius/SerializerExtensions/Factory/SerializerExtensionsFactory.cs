using System;
using System.Collections.Generic;
using System.Reflection;

namespace Archivarius
{
    public class SerializerExtensionsFactory : ISerializerExtensionsFactory
    {
        public static readonly SerializerExtensionsFactory Instance;

        private static bool _isRegisteredInAOTGuard;
        private static readonly object _lock = new object();

        private readonly SerializerExtensionsFactoryMode _aotMode;
        private readonly IAOTGuardChecker? _aotGuard;

        private readonly Dictionary<Type, ISerializerExtension?> _extensions = new Dictionary<Type, ISerializerExtension?>();
        private readonly Dictionary<Type, Func<Type, ISerializerExtension?>> _ctors1 = new Dictionary<Type, Func<Type, ISerializerExtension?>>();
        private readonly Dictionary<Type, Func<Type, Type, ISerializerExtension?>> _ctors2 = new Dictionary<Type, Func<Type, Type, ISerializerExtension?>>();

        public event Action<Type, Exception>? OnError;

        static SerializerExtensionsFactory()
        {
            Instance = new SerializerExtensionsFactory(SerializerExtensionsFactoryMode.AOTUnsafeMode);
        }

        public SerializerExtensionsFactory(SerializerExtensionsFactoryMode aotMode)
        {
            switch (aotMode)
            {
                case SerializerExtensionsFactoryMode.JITMode:
                case SerializerExtensionsFactoryMode.AOTSafeMode:
                    _aotGuard = null;
                    break;
                case SerializerExtensionsFactoryMode.AOTUnsafeMode:
                    _aotGuard = AOTGuard.Instance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aotMode), aotMode, null);
            }

            _aotMode = aotMode;

            Register0(new BoolSerializerExtension());
            Register0(new ByteSerializerExtension());
            Register0(new SByteSerializerExtension());
            Register0(new CharSerializerExtension());
            Register0(new ShortSerializerExtension());
            Register0(new UShortSerializerExtension());
            Register0(new IntSerializerExtension());
            Register0(new UIntSerializerExtension());
            Register0(new LongSerializerExtension());
            Register0(new ULongSerializerExtension());
            Register0(new FloatSerializerExtension());
            Register0(new DoubleSerializerExtension());
            Register0(new StringSerializerExtension());

            Register0(new GuidSerializerExtension());
            Register0(new DateTimeSerializerExtension());

            AOTGuard.Instance.RegisterType(new DataClassSerializerExtension<IDataStruct>());
            AOTGuard.Instance.RegisterType(new ArraySerializerExtension<Object>(null!));

            if (_aotMode != SerializerExtensionsFactoryMode.AOTSafeMode)
            {
                Register1(typeof(List<>), typeof(ListSerializerExtension<>));
                Register1(typeof(Stack<>), typeof(StackSerializerExtension<>));
                Register1(typeof(Queue<>), typeof(QueueSerializerExtension<>));
                Register1(typeof(HashSet<>), typeof(HashSetSerializerExtension<>));
                Register2(typeof(Dictionary<,>), typeof(DictionarySerializerExtension<,>));

                if (_aotMode == SerializerExtensionsFactoryMode.AOTUnsafeMode)
                {
                    if (!_isRegisteredInAOTGuard)
                    {
                        lock (_lock)
                        {
                            if (!_isRegisteredInAOTGuard)
                            {
                                RegisterTypesInAOTGuard();
                                _isRegisteredInAOTGuard = true;
                            }
                        }
                    }
                }
            }
        }

        public void Register0<T>(ISerializerExtension<T> extension)
        {
            _extensions.Add(typeof(T), extension);
        }

        public void Register1(Type genericTypeDef, Type genericExtensionType)
        {
            if (_aotMode == SerializerExtensionsFactoryMode.AOTSafeMode)
            {
                throw new InvalidOperationException();
            }

            if (!genericTypeDef.IsGenericTypeDefinition || genericTypeDef.GetGenericArguments().Length != 1)
            {
                throw new InvalidOperationException();
            }

            if (!genericExtensionType.IsGenericTypeDefinition || genericExtensionType.GetGenericArguments().Length != 1)
            {
                throw new InvalidOperationException();
            }

            ISerializerExtension CtorMethod(Type paramType)
            {
                ISerializerExtension? nestedExtension = ((ISerializerExtensionsFactory)this).Construct(paramType);

                if (nestedExtension == null)
                {
                    throw new InvalidOperationException("Generic parameter type #1 is not supported");
                }

                Type realExtensionType = genericExtensionType.MakeGenericType(paramType);
                _aotGuard?.CheckType(realExtensionType);

                var realExtensionCtor = realExtensionType.GetConstructor(new Type[]
                {
                    typeof(ISerializerExtension<>).MakeGenericType(paramType)
                });
                if (realExtensionCtor == null)
                {
                    throw new InvalidOperationException();
                }
                return (ISerializerExtension)realExtensionCtor.Invoke(new Object[] { nestedExtension });
            }

            _ctors1.Add(genericTypeDef, CtorMethod);
        }

        public void Register2(Type genericTypeDef, Type genericExtensionType)
        {
            if (_aotMode == SerializerExtensionsFactoryMode.AOTSafeMode)
            {
                throw new InvalidOperationException();
            }

            if (!genericTypeDef.IsGenericTypeDefinition || genericTypeDef.GetGenericArguments().Length != 2)
            {
                throw new InvalidOperationException();
            }

            if (!genericExtensionType.IsGenericTypeDefinition || genericExtensionType.GetGenericArguments().Length != 2)
            {
                throw new InvalidOperationException();
            }

            ISerializerExtension? CtorMethod(Type param1Type, Type param2Type)
            {
                ISerializerExtension? nestedExtension1 = ((ISerializerExtensionsFactory)this).Construct(param1Type);
                ISerializerExtension? nestedExtension2 = ((ISerializerExtensionsFactory)this).Construct(param2Type);

                if (nestedExtension1 == null)
                {
                    throw new InvalidOperationException("Generic parameter type #1 is not supported");
                }

                if (nestedExtension2 == null)
                {
                    throw new InvalidOperationException("Generic parameter type #2 is not supported");
                }

                Type realExtensionType = genericExtensionType.MakeGenericType(param1Type, param2Type);
                _aotGuard?.CheckType(realExtensionType);

                var realExtensionCtor = realExtensionType.GetConstructor(new Type[]
                {
                    typeof(ISerializerExtension<>).MakeGenericType(param1Type),
                    typeof(ISerializerExtension<>).MakeGenericType(param2Type)
                });
                return realExtensionCtor?.Invoke(new object[] {nestedExtension1, nestedExtension2}) as ISerializerExtension;
            }

            _ctors2.Add(genericTypeDef, CtorMethod);
        }

        public ISerializerExtension? Construct(Type type)
        {
            if (_extensions.TryGetValue(type, out var extension))
            {
                return extension;
            }

            ISerializerExtension? result = null;
            try
            {
                if (typeof(IDataStruct).IsAssignableFrom(type))
                {
                    Type t;
                    if (!type.IsValueType)
                    {
                        t = typeof(DataClassSerializerExtension<>);
                    }
                    else if (typeof(IVersionedDataStruct).IsAssignableFrom(type))
                    {
                        t = typeof(VersionedDataStructSerializerExtension<>);
                    }
                    else
                    {
                        t = typeof(DataStructSerializerExtension<>);
                    }

                    t = t.MakeGenericType(type);
                    _aotGuard?.CheckType(t);

                    result = (ISerializerExtension)t.GetConstructor(Type.EmptyTypes)!.Invoke(Array.Empty<Object>());

                }
                else if (type.IsArray)
                {
                    int rank = type.GetArrayRank();
                    if (rank == 1)
                    {
                        Type? elementType = type.GetElementType();
                        ISerializerExtension? elementSerializer = elementType != null ? Construct(elementType) : null;
                        if (elementSerializer != null)
                        {
                            Type t = typeof(ArraySerializerExtension<>).MakeGenericType(elementType!);
                            _aotGuard?.CheckType(t);
                            result = (ISerializerExtension)t.GetConstructor(new Type[]
                                {
                                    typeof(ISerializerExtension<>)!.MakeGenericType(elementType!)
                                })!.Invoke(new Object[] { elementSerializer });
                        }
                        else
                        {
                            throw new InvalidOperationException("Array element type is not supported");
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Multidimensional arrays are not supported yet");
                    }
                }
                else if (type.IsGenericType)
                {
                    Type genericTypeDef = type.GetGenericTypeDefinition();
                    Type[] arguments = type.GetGenericArguments();
                    switch (arguments.Length)
                    {
                        case 1:
                            if (_ctors1.TryGetValue(genericTypeDef, out var ctor1))
                            {
                                result = ctor1(arguments[0]);
                            }

                            break;
                        case 2:
                            if (_ctors2.TryGetValue(genericTypeDef, out var ctor2))
                            {
                                result = ctor2(arguments[0], arguments[1]);
                            }

                            break;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unsupported type scheme");
                }
            }
            catch (Exception ex)
            {
                result = null;
                OnError?.Invoke(type, ex);
            }

            _extensions.Add(type, result);
            return result;
        }

        public ISerializerExtension<T>? Construct<T>()
        {
            return Construct(typeof(T)) as ISerializerExtension<T>;
        }

        private static void RegisterTypesInAOTGuard()
        {
            DefaultExtensionAOTRegister defaultExtensionRegister = new DefaultExtensionAOTRegister();
            defaultExtensionRegister.RegisterTypes();

            Type register = typeof(IExtensionsAOTRegister);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GetName().Name!.StartsWith("System"))
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && register.IsAssignableFrom(type))
                        {
                            var obj = type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<Object>());
                            type.GetMethod(nameof(IExtensionsAOTRegister.RegisterTypes))!.Invoke(obj, Array.Empty<Object>());
                        }
                    }
                }
            }
        }
    }
}