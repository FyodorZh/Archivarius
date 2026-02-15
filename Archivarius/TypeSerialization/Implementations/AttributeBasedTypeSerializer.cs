using System;
using System.Collections.Generic;

namespace Archivarius
{
    public abstract class AttributeBasedTypeSerializer<TId> : ITypeSerializer
    {
        protected abstract List<TId> GetTmpList();
        protected abstract void WriteId(IWriter writer, TId id);
        protected abstract TId GetTypeId(Type type);
        
        
        public void Serialize(IWriter writer, Type type)
        {
            var ids = GetTmpList();
            EncodeTypeRecursive(ids, type);
            
            writer.WriteByte(0); // version for the future
            writer.WriteByte(checked((byte)ids.Count));
            foreach (var id in ids)
            {
                WriteId(writer, id);
            }
        }

        private void EncodeTypeRecursive(List<TId> ids, Type type)
        {
            TId typeId = GetTypeId(type);
            ids.Add(typeId);
            if (type.IsGenericType)
            {
                foreach (var argument in type.GenericTypeArguments)
                {
                    EncodeTypeRecursive(ids, argument);
                }
            }
        }
    }
}