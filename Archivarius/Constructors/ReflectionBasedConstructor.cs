using System;
using System.Reflection;

namespace Archivarius.Constructors
{
    public class ReflectionBasedConstructor : IConstructor
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
}