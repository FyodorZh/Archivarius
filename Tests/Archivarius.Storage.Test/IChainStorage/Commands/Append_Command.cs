using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class Append_Command<TData> : ChainStorageTestCommand<TData, int>
        where TData : class, IDataStruct
    {
        private readonly TData _data;
        
        public Append_Command(TData data)
        {
            _data = data;
        }
        
        protected override Task<int> InvokeOnSubject(IChainStorage<TData> subject)
        {
            return subject.Append(_data);
        }
    }
}