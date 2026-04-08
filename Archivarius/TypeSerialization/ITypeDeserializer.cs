using System;

namespace Archivarius
{
    public interface ITypeDeserializer
    {
        Type? Deserialize(ILowLevelReader reader);
    }
}