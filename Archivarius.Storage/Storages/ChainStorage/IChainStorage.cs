using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public interface IReadOnlyChainStorage<TData>
        where TData : class, IDataStruct
    {
        Task ClearCache();
        Task<int> GetCount();
        Task<TData?> GetAt(int id);
        IAsyncEnumerable<IReadOnlyList<TData>> GetAll();
        IAsyncEnumerable<IReadOnlyList<TData>> GetMany(int from, int till = -1);
    }
    
    public interface IChainStorage<TData> : IReadOnlyChainStorage<TData>
        where TData : class, IDataStruct
    {
        Task<int> Append(TData data);
        Task RewriteData(bool includeIndex, bool includePacks, bool includeElements, Action<int>? progress = null);
    }
}