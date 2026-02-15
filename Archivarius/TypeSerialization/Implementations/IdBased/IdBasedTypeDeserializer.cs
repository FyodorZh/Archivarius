using System;
using System.Collections.Generic;
using Archivarius.TypeSerializers;

namespace Archivarius
{
    public class IdBasedTypeDeserializer : AttributeBasedTypeDeserializer<ushort>
    {
        private static readonly Type _attrType = typeof(IdAttribute);
        
        [ThreadStatic] private static List<ushort>? _ids;
        
        protected override List<ushort> GetTmpList()
        {
            _ids ??= new List<ushort>();
            _ids.Clear();
            return _ids;
        }

        protected override bool GetTypeId(Type type, out ushort id)
        {
            var attribute = Attribute.GetCustomAttribute(type, _attrType, false) as IdAttribute;
            id = attribute?.Value ?? 0;
            return attribute != null;
        }

        protected override ushort ReadId(IReader reader)
        {
            return reader.ReadUShort();
        }

        protected override Type? GetSystemTypeById(ushort id)
        {
            switch (id)
            {
                case 1: return typeof(Boolean);
                case 2: return typeof(Byte);
                case 3: return typeof(SByte);
                case 4: return typeof(Int16);
                case 5: return typeof(UInt16);
                case 6: return typeof(Int32);
                case 7: return typeof(UInt32);
                case 8: return typeof(Int64);
                case 9: return typeof(UInt64);
                case 10: return typeof(Char);
                case 11: return typeof(Single);
                case 12: return typeof(Double);
                case 13: return typeof(Decimal);
                case 14: return typeof(String);
                default: return null;
            }
        }
    }
}