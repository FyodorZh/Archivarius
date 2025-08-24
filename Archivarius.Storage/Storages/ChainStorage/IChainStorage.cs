using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public interface IChainStorage<TData>
        where TData : class, IDataStruct
    {
        Task<int> GetCount();
        Task<TData?> GetAt(int id);
        IAsyncEnumerable<IReadOnlyList<TData>> GetAll();
        IAsyncEnumerable<IReadOnlyList<TData>> GetMany(int from, int till = -1);
        Task<int> Append(TData data);
        Task RewriteData(bool includeIndex, bool includePacks, bool includeElements, Action<int>? progress = null);
    }
}