using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public static class ChainStorage_Factory
    {
        public static async Task<IChainStorage<TData>?> CreateNewChain<TData>(this IKeyValueStorage storage, DirPath path,
            int packSize = 1000)
            where TData : class, IDataStruct
        {
            var list = await storage.GetElements(path);
            if (list.Count > 0)
            {
                return null;
            }

            return ChainStorage<TData>.ConstructNew(storage, path, packSize);
        }
        
        public static async Task<IChainStorage<TData>?> OpenChain<TData>(this IKeyValueStorage storage, DirPath path)
            where TData : class, IDataStruct
        {
            var chain = ChainStorage<TData>.LoadFrom(storage, path);
            if (await chain.IsValid())
            {
                return chain;
            }

            return null;
        }
    }
}