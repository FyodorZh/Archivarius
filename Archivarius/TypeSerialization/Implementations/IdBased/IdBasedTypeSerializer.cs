using System;
using System.Collections.Generic;

namespace Archivarius
{
    public class IdBasedTypeSerializer : AttributeBasedTypeSerializer<ushort>
    {
        private static readonly Type _attrType = typeof(IdAttribute);
        
        [ThreadStatic] private static List<ushort>? _ids;
        
        protected override List<ushort> GetTmpList()
        {
            _ids ??= new List<ushort>();
            _ids.Clear();
            return _ids;
        }
        
        protected override void WriteId(IWriter writer, ushort id)
        {
            writer.WriteUShort(id);
        }

        protected override ushort GetTypeId(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:  return 1;
                case TypeCode.Byte:     return 2;
                case TypeCode.SByte:    return 3;
                case TypeCode.Int16:    return 4;
                case TypeCode.UInt16:   return 5;
                case TypeCode.Int32:    return 6;
                case TypeCode.UInt32:   return 7;
                case TypeCode.Int64:    return 8;
                case TypeCode.UInt64:   return 9;
                case TypeCode.Char:     return 10;
                case TypeCode.Single:   return 11;
                case TypeCode.Double:   return 12;
                case TypeCode.Decimal:  return 13;
                case TypeCode.String:   return 14;
            }

            if (Attribute.GetCustomAttribute(type, _attrType, false) is not IdAttribute attribute)
            {
                throw new InvalidOperationException($"'{type}' must have GUID attribute");
            }

            return attribute.Value;
        }
    }
}