using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class GetCount_Command<TData> : ChainStorageTestCommand<TData, int>
        where TData : class, IDataStruct
    {
        protected override Task<int> InvokeOnSubject(ChainStorageWrapper<TData> subject)
        {
            return subject.Storage.GetCount();
        }
    }
}