using System;

namespace Archivarius.TypeSerializers
{
    public class TypenameBasedTypeSerializer : ITypeSerializer
    {
        public void Serialize(IWriter writer, Type type)
        {
            string typeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException();
            writer.WriteString(typeName);
        }
    }
}