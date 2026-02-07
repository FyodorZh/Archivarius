using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class ClearCache_Command<TData>  : ChainStorageTestCommand<TData, bool>
        where TData : class, IDataStruct
    {
        protected override async Task<bool> InvokeOnSubject(IChainStorage<TData> subject)
        {
            await subject.ClearCache();
            return true;
        }
    }
}