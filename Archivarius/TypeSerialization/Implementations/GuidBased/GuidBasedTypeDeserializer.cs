using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Archivarius.TypeSerializers
{
    public class GuidBasedTypeDeserializer : AttributeBasedTypeDeserializer<string>
    {
        private static readonly Type _attrType = typeof(GuidAttribute);
        
        [ThreadStatic] private static List<string>? _ids;
        
        protected override List<string> GetTmpList()
        {
            _ids ??= new List<string>();
            _ids.Clear();
            return _ids;
        }

        protected override bool GetTypeId(Type type, out string id)
        {
            var attribute = Attribute.GetCustomAttribute(type, _attrType, false) as GuidAttribute;
            id = attribute?.Value ?? "invalid-guid";
            return attribute != null;
        }

        protected override string ReadId(IReader reader)
        {
            return reader.ReadString() ?? "null";
        }

        protected override Type? GetSystemTypeById(string id)
        {
            switch (id)
            {
                case "Boolean": return typeof(Boolean);
                case "Byte": return typeof(Byte);
                case "SByte": return typeof(SByte);
                case "Int16": return typeof(Int16);
                case "UInt16": return typeof(UInt16);
                case "Int32": return typeof(Int32);
                case "UInt32": return typeof(UInt32);
                case "Int64": return typeof(Int64);
                case "UInt64": return typeof(UInt64);
                case "Char": return typeof(Char);
                case "Single": return typeof(Single);
                case "Double": return typeof(Double);
                case "Decimal": return typeof(Decimal);
                case "String": return typeof(String);
                default: return null;
            }
        }
    }
}