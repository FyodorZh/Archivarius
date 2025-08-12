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
        Task<int> Append(TData data);
    }
}