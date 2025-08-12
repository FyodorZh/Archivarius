using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public static class ChainStorage_Factory
    {
        public static async Task<IChainStorage<TData>?> InitNewChain<TData>(this IKeyValueStorage storage, DirPath path,
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
            if (await storage.IsExists(path.File("index")))
            {
                return ChainStorage<TData>.LoadFrom(storage, path);
            }

            return null;
        }
    }
}