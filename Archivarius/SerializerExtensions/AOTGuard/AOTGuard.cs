using System;
using System.Collections.Concurrent;

namespace Archivarius
{
    public interface IAOTGuardChecker
    {
        void CheckType(Type type);
    }

    public interface IAOTGuard : IAOTGuardChecker
    {
        event Action<Type> OnFail;
        bool IsActive { get; set; }
        void RegisterType<T>(T value);
    }

    public class AOTGuard : IAOTGuard
    {
        public static readonly IAOTGuard Instance = new AOTGuard();
        
        private enum KnowledgeType
        {
            ComplexGeneric,
            SimpleGenericNoClass,
            SimpleGenericWithClass
        }

        private readonly ConcurrentBag<Type> _registeredTypes = new ConcurrentBag<Type>();

        private readonly ConcurrentDictionary<Type, KnowledgeType> _knownGenericTypes = new ConcurrentDictionary<Type, KnowledgeType>();
        private readonly ConcurrentDictionary<Type, bool> _knownTypes = new ConcurrentDictionary<Type, bool>();
        private readonly ConcurrentDictionary<Type, bool> _unknownTypes = new ConcurrentDictionary<Type, bool>();


        public event Action<Type>? OnFail;

        public bool IsActive { get; set; } = false;

        private AOTGuard()
        {
        }

        public void RegisterType<T>(T value)
        {
            Type type = value!.GetType();
            _registeredTypes.Add(type);
            if (IsActive)
            {
                if (!type.IsGenericType)
                {
                    throw new InvalidOperationException("Type must be generic");
                }

                _unknownTypes.TryRemove(type, out var _);

                if (_knownTypes.TryAdd(type, true))
                {
                    var list = type.GetGenericArguments();
                    if (list.Length > 1)
                    {
                        _knownGenericTypes.TryAdd(type.GetGenericTypeDefinition(), KnowledgeType.ComplexGeneric);
                    }
                    else
                    {
                        if (!list[0].IsValueType)
                        {
                            _knownGenericTypes.AddOrUpdate(type.GetGenericTypeDefinition(), KnowledgeType.SimpleGenericWithClass,
                                (type1, knowledgeType) => KnowledgeType.SimpleGenericWithClass);
                        }
                        else
                        {
                            _knownGenericTypes.TryAdd(type.GetGenericTypeDefinition(), KnowledgeType.SimpleGenericNoClass);
                        }
                    }
                }
            }
        }

        public void CheckType(Type type)
        {
            if (IsActive)
            {
                if (!_knownTypes.TryGetValue(type, out var _))
                {
                    if (_unknownTypes.TryAdd(type, true))
                    {
                        Type[] paramTypes = type.GetGenericArguments();
                        if (paramTypes.Length > 1 || paramTypes[0].IsValueType ||
                            !_knownGenericTypes.TryGetValue(type.GetGenericTypeDefinition(), out var knownType) ||
                            knownType != KnowledgeType.SimpleGenericWithClass)
                        {
                            OnFail?.Invoke(type);
                        }
                    }
                }
            }
        }
    }
}