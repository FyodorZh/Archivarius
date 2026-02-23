using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test
{
    public class ChainStorageWrapper<TData>
        where TData : class, IDataStruct
    {
        private readonly Func<Task<IChainStorage<TData>>> _factory;
        
        public IChainStorage<TData> Storage { get; private set; }

        public ChainStorageWrapper(Func<Task<IChainStorage<TData>>> factory)
        {
            _factory = factory;
        }

        public async Task Reinit()
        {
            Storage = await _factory.Invoke();
        }
    }
}