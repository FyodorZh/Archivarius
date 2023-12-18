using System;
using System.Runtime.InteropServices;

namespace Archivarius
{
    public class GuidSerializerExtension : ISerializerExtension<Guid>
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct Alias
        {
            [FieldOffset(0)] public Guid Guid;

            [FieldOffset(0)] public ulong A;
            [FieldOffset(8)] public ulong B;
        }

        public void Add(ISerializer serializer, ref Guid value)
        {
            if (serializer.IsWriter)
            {
                Alias rec = new Alias() { Guid = value };
                serializer.Add(ref rec.A);
                serializer.Add(ref rec.B);
            }
            else
            {
                Alias rec = default;
                serializer.Add(ref rec.A);
                serializer.Add(ref rec.B);
                value = rec.Guid;
            }
        }
    }
}