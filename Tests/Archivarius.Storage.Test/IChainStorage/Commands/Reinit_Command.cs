using System.Threading.Tasks;
using Pontifex.Api;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class Reinit_Command<TData> : ChainStorageTestCommand<TData, EmptyMessage>
        where TData : class, IDataStruct
    {
        protected override async Task<EmptyMessage> InvokeOnSubject(ChainStorageWrapper<TData> subject)
        {
            await subject.Reinit();
            return new EmptyMessage();
        }
    }
}