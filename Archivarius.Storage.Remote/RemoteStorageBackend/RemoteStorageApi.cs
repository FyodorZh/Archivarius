using Archivarius.DataModels;
using Pontifex.Api;

namespace Archivarius.Storage.Remote
{
    internal class RemoteStorageApi : ApiRoot
    {
        public readonly RRDecl<StringWrapper, StructsArray<StringWrapper>> GetSubPath = new ();
        public readonly RRDecl<StringWrapper, BoolWrapper> IsExists = new ();
        public readonly RRDecl<StringWrapper, BytesWrapper> Read = new ();
        
        public readonly RRDecl<Pair<StringWrapper, BytesWrapper>, BoolWrapper> Write = new ();
        public readonly RRDecl<StringWrapper, BoolWrapper> Delete = new ();
    }
}