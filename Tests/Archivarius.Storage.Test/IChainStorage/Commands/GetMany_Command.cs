using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class GetMany_Command<TData> : ChainStorageTestCommand<TData, DataArray<TData>>
        where TData : class, IDataStruct
    {
        private readonly double _from;
        private readonly double _till;
        
        public GetMany_Command(double from, double till)
        {
            _from = from;
            _till = till;
        }
        
        protected override async Task<DataArray<TData>> InvokeOnSubject(IChainStorage<TData> subject)
        {
            int count = await subject.GetCount();
            int from = (int)(_from * count);
            int till = (int)(_from + (1 - _from) * _till * count);
            
            from = Math.Min(from, count - 1);
            till = Math.Min(till, count - 1);
            from = Math.Max(0, from);
            till = Math.Max(0, till);
            
            DataArray<TData> res = new();
            await foreach (var data in subject.GetMany(from, till))
            {
                res.Data.AddRange(data);
            }

            return res;
        }
    }
}