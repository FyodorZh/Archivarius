using System;

namespace Archivarius.Constructors
{
    public class DefaultCtorTypeConstructorFactory : IConstructorFactory
    {
        public IConstructor Build(Type type)
        {
            if (type.GetConstructor(Type.EmptyTypes) != null)
            {
                Type genericCtor = typeof(GenericTypeConstructor<>);
                Type typeCtor = genericCtor.MakeGenericType(type);
                return (IConstructor)typeCtor.GetConstructor(Type.EmptyTypes)!.Invoke([]);
            }

            return new ReflectionBasedConstructor(type);
        }
    }
}