using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class InMemoryChainStorage<TData> : IChainStorage<TData> 
        where TData : class, IDataStruct
    {
        private readonly List<TData> _data = new();
        
        public Task ClearCache()
        {
            // DO NOTHING
            return Task.CompletedTask;
        }

        public Task<int> GetCount()
        {
            return Task.FromResult(_data.Count);
        }

        public Task<TData?> GetAt(int id)
        {
            if (id < _data.Count)
            {
                return Task.FromResult<TData?>(_data[id]);
            }
            return Task.FromResult<TData?>(null);
        }

        public async IAsyncEnumerable<IReadOnlyList<TData>> GetAll()
        {
            yield return _data.ToArray();
        }

        public async IAsyncEnumerable<IReadOnlyList<TData>> GetMany(int from, int till = -1)
        {
            if (_data.Count == 0)
            {
                yield break;
            }

            if (till < 0)
            {
                till = _data.Count - 1;
            }

            if (from < 0 || from > till || till >= _data.Count)
            {
                throw new Exception();
            }
            
            int count = till - from + 1;

            yield return _data.GetRange(from, count);
        }

        public Task<int> Append(TData data)
        {
            _data.Add(data);
            return Task.FromResult(_data.Count - 1);
        }

        public Task RewriteData(bool includeIndex, bool includePacks, bool includeElements, Action<int>? progress = null)
        {
            return Task.CompletedTask;
        }
    }
}