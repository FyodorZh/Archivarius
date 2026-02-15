using System;

namespace Archivarius
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class IdAttribute : Attribute
    {
        public byte GroupId { get; }
        public byte IdInGroup { get; }
        public ushort Value => (ushort)((GroupId << 8) | IdInGroup);
        
        public IdAttribute(byte groupId, byte idInGroup)
        {
            if (groupId == 0)
            {
                throw new InvalidOperationException("Zero group is reserved for internal usage");
            }
            GroupId = groupId;
            IdInGroup = idInGroup;
        }
    }
}