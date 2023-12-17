using System;

namespace Archivarius.TypeSerializers
{
    public class TypenameBasedTypeDeserializer : ITypeDeserializer
    {
        public Type? Deserialize(IReader reader)
        {
            string? typeName = reader.ReadString();
            var type = typeName != null ? Type.GetType(typeName) : null;
            return type;
        }
    }
}