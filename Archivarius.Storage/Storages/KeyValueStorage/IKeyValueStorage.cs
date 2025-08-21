using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public interface IKeyValueStorage
    {
        IKeyValueStorage SubDirectory(DirPath path);
        Task Set<TData>(FilePath path, TData data) where TData : class, IDataStruct;
        Task SetStruct<TData>(FilePath path, TData data) where TData : struct, IDataStruct;
        Task SetVersionedStruct<TData>(FilePath path, TData data) where TData : struct, IVersionedDataStruct;
        Task<TData?> Get<TData>(FilePath path) where TData : class, IDataStruct;
        Task<TData?> GetStruct<TData>(FilePath path) where TData : struct, IDataStruct;
        Task<TData?> GetVersionedStruct<TData>(FilePath path) where TData : struct, IVersionedDataStruct;
        Task Erase(FilePath path);
        Task<bool> IsExists(FilePath path);
        Task<IReadOnlyCollection<FilePath>> GetElements(DirPath path);
    }
}