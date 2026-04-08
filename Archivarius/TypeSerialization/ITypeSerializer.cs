using System;

namespace Archivarius
{
    public interface ITypeSerializer
    {
        void Serialize(ILowLevelWriter writer, Type type);
    }
}