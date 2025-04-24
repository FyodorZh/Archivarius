namespace Archivarius
{
    public interface ILowLevelReader
    {
        bool ReadBool();
        byte ReadByte();
        char ReadChar();
        short ReadShort();
        int ReadInt();
        long ReadLong();
        float ReadFloat();
        double ReadDouble();
        decimal ReadDecimal();
        string? ReadString();
        byte[]? ReadBytes();
    }

    public interface IReader : ILowLevelReader
    {
        bool TrySetSectionUsage(bool useSections);
        void BeginSection();
        bool EndSection();
    }
    
    
    public static class ILowLevelReader_Ext
    {
        public static sbyte ReadSByte(this ILowLevelReader reader)
        {
            var res = reader.ReadByte();
            return unchecked((sbyte)res);
        }

        public static ushort ReadUShort(this ILowLevelReader reader)
        {
            var res = reader.ReadShort();
            return unchecked((ushort)res);
        }

        public static uint ReadUInt(this ILowLevelReader reader)
        {
            var res = reader.ReadInt();
            return unchecked((uint)res);
        }

        public static ulong ReadULong(this ILowLevelReader reader)
        {
            var res = reader.ReadLong();
            return unchecked((ulong)res);
        }
    }
}