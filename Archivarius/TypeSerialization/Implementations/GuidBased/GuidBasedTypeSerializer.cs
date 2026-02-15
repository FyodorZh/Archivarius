using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Archivarius.TypeSerializers
{
    public class GuidBasedTypeSerializer : AttributeBasedTypeSerializer<string>
    {
        private static readonly Type _attrType = typeof(GuidAttribute);
        
        [ThreadStatic] private static List<string>? _ids;
        
        protected override List<string> GetTmpList()
        {
            _ids ??= new List<string>();
            _ids.Clear();
            return _ids;
        }

        protected override void WriteId(IWriter writer, string id)
        {
            writer.WriteString(id);
        }

        protected override string GetTypeId(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:  return "Boolean";
                case TypeCode.Byte:     return "Byte";
                case TypeCode.SByte:    return "SByte";
                case TypeCode.Int16:    return "Int16";
                case TypeCode.UInt16:   return "UInt16";
                case TypeCode.Int32:    return "Int32";
                case TypeCode.UInt32:   return "UInt32";
                case TypeCode.Int64:    return "Int64";
                case TypeCode.UInt64:   return "UInt64";
                case TypeCode.Char:     return "Char";
                case TypeCode.Single:   return "Single";
                case TypeCode.Double:   return "Double";
                case TypeCode.Decimal:  return "Decimal";
                case TypeCode.String:   return "String";
            }

            if (Attribute.GetCustomAttribute(type, _attrType, false) is not GuidAttribute attribute)
            {
                throw new InvalidOperationException($"'{type}' must have GUID attribute");
            }

            return attribute.Value;
        }
    }
}