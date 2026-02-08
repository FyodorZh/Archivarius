using System;

namespace Archivarius.Constructors
{
    public interface IConstructorFactory
    {
        IConstructor Build(Type type);
    }
}