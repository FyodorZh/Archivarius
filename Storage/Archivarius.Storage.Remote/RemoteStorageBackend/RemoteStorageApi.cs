using System;
using Actuarius.Memory;
using Archivarius.DataModels;
using Pontifex.Api;

namespace Archivarius.Storage.Remote
{
    public class RemoteStorageApi : ApiRoot
    {
        public readonly RRDecl<StringWrapper, StructsArray<StringWrapper>> GetSubPath = new ();
        public readonly RRDecl<StringWrapper, BoolWrapper> IsExists = new ();
        public readonly RRDecl<StringWrapper, MultiRefByteArrayWrapper> Read = new ();
        
        public readonly RRDecl<Pair<StringWrapper, MultiRefByteArrayWrapper>, BoolWrapper> Write = new ();
        public readonly RRDecl<StringWrapper, BoolWrapper> Delete = new ();
    }

    public struct MultiRefByteArrayWrapper : IDataStruct
    {
        public IMultiRefByteArray? Value;

        public void Serialize(ISerializer serializer)
        {
            if (serializer.IsWriter)
            {
                if (Value == null)
                {
                    serializer.Writer.WriteInt(0);
                }
                else
                {
                    int count = Value.Count;
                    serializer.Writer.WriteInt(count + 1);
                    serializer.Writer.WriteBytes(Value.Array, Value.Offset, count);
                }
            }
            else
            {
                Value?.Release();
                
                int count = serializer.Reader.ReadInt();
                if (count == 0)
                {
                    Value = null;
                }
                else
                {
                    count -= 1;
                    Value = MemoryRental.Shared.ByteArraysPool.Acquire(count);
                    serializer.Reader.ReadBytes(Value.Array, Value.Offset, count);
                }
            }
        }
    }
}