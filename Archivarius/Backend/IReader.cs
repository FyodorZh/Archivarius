using System;
using System.Threading.Tasks;

namespace Archivarius
{
    public interface ILowLevelReader
    {
        bool ReadBool();
        byte ReadByte();
        sbyte ReadSByte();
        char ReadChar();
        short ReadShort();
        ushort ReadUShort();
        int ReadInt();
        uint ReadUInt();
        long ReadLong();
        ulong ReadULong();
        float ReadFloat();
        double ReadDouble();
        decimal ReadDecimal();
        string? ReadString();
        byte[]? ReadArray();
        void ReadBytes(byte[] dst, int offset, int count);
    }

    public interface IReader : ILowLevelReader
    {
        /// <summary>
        /// Some readers may load data to read asynchronously.
        /// This method ensures that all required data till the next Preload() invocation is loaded.
        /// </summary>
        /// <returns>
        /// TRUE - Reader has some data to read
        /// FALSE - Reader has no data. The Deserialization process must stop.
        /// </returns>
        ValueTask<bool> Preload();
        bool TrySetSectionUsage(bool useSections);
        void BeginSection();
        bool EndSection();
    }
    
}

