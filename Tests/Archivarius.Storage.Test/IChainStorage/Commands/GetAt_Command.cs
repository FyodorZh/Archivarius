using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class GetAt_Command<TData> : ChainStorageTestCommand<TData, TData?>
        where TData : class, IDataStruct, IEquatable<TData>
    {
        private readonly double _index;
        
        public GetAt_Command(double index)
        {
            _index = index;
        }
        
        protected override async Task<TData?> InvokeOnSubject(ChainStorageWrapper<TData> subject)
        {
            int count = await subject.Storage.GetCount();
            int index = (int)(count * _index);
            return await subject.Storage.GetAt(index);
        }
    }
}