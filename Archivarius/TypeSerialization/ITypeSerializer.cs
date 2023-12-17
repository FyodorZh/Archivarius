using System;

namespace Archivarius
{
    public interface ITypeSerializer
    {
        void Serialize(IWriter writer, Type type);
    }
}