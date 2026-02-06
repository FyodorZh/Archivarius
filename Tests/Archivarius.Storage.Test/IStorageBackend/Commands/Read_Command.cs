using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class Read_Command : StorageBackendTestCommand<Read_Command.ByteArray>
    {
        private readonly FilePath _path;

        public Read_Command(FilePath path)
        {
            _path = path;
        }

        protected override async Task<ByteArray> InvokeOnSubject(IStorageBackend subject)
        {
            List<byte> bytes = new(); 
            await subject.Read(_path, 0, (s, _) =>
            {
                int iByte;
                while ((iByte = s.ReadByte()) >= 0)
                {
                    bytes.Add((byte)iByte);
                }
                return Task.CompletedTask;
            });
            return new ByteArray() { Bytes = bytes };
        }

        public struct ByteArray : IEquatable<ByteArray>
        {
            public List<byte> Bytes = [];
            public ByteArray() { }

            public bool Equals(ByteArray other)
            {
                return Bytes.SequenceEqual(other.Bytes);
            }
        }
    }
}