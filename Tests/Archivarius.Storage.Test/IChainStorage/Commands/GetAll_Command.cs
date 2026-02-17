using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class GetAll_Command<TData> : ChainStorageTestCommand<TData, DataArray<TData>>
        where TData : class, IDataStruct
    {
        protected override async Task<DataArray<TData>> InvokeOnSubject(ChainStorageWrapper<TData> subject)
        {
            DataArray<TData> res = new();
            await foreach (var data in subject.Storage.GetAll())
            {
                res.Data.AddRange(data);
            }

            return res;
        }
    }
}