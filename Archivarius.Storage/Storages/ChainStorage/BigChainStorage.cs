using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class BigReadOnlyChainStorage<TData> : IReadOnlyChainStorage<TData>
        where TData : class, IDataStruct
    {
        private readonly IReadOnlyKeyValueStorage _storage;
        protected readonly DirPath _rootPath;
        
        protected readonly SemaphoreSlim _locker = new(1, 1);
        
        protected const string _indexName = "bigchain.index";

        private readonly bool _noIndexCache;
        protected bool _hasIndex;
        protected IndexData _index;
        
        protected int _cachedSmallPackId = -1;
        protected SmallPackData? _cachedSmallPack;

        protected int _cachedBigPackId = -1;
        protected BigPackData? _cachedBigPack;
        
        public static BigReadOnlyChainStorage<TData> LoadFrom(IReadOnlyKeyValueStorage storage, DirPath path, bool noCache = false)
        {
            return new(storage, path, noCache);
        }

        protected BigReadOnlyChainStorage(IReadOnlyKeyValueStorage storage, DirPath rootPath, bool noIndexCache)
        {
            _storage = storage;
            _rootPath = rootPath;
            _index = default;
            _hasIndex = false;
            _noIndexCache = noIndexCache;
        }

        public async Task<bool> IsValid()
        {
            var indexFile = _rootPath.File(_indexName);
            if (await _storage.IsExists(indexFile)) // check to eliminate error logs
            {
                var index = await _storage.GetVersionedStruct<IndexData>(indexFile);
                return index != null;
            }
            return false;
        }

        protected async ValueTask<IndexData> GetIndex_Unsafe()
        {
            if (_noIndexCache || !_hasIndex)
            {
                var index = await _storage.GetVersionedStruct<IndexData>(_rootPath.File(_indexName));
                if (index != null)
                {
                    _index = index.Value;
                    _hasIndex = true;
                }
            }

            if (!_hasIndex)
            {
                throw new Exception();
            }
            return _index;
        }

        private async Task<BigPackData> GetBigPack_Unsafe_NoCache(int bigPackId)
        {
            string packName = string.Format(_index.BigPackName, bigPackId);
            var packPath = _rootPath.File(packName);
            return await _storage.GetStruct<BigPackData>(packPath) ?? throw new Exception($"Failed to load '{string.Format(_index.BigPackName, bigPackId)}'");
        }

        protected async ValueTask<BigPackData> GetBigPack_Unsafe(int bigPackId)
        {
            if (_cachedBigPackId != bigPackId || _cachedBigPack == null)
            {
                _cachedBigPack = await GetBigPack_Unsafe_NoCache(bigPackId);
                _cachedBigPackId = bigPackId;
            }
            return _cachedBigPack.Value;
        }

        private async Task<SmallPackData> GetSmallPack_Unsafe_NoCache(int smallPackId)
        {
            string packName = string.Format(_index.SmallPackName, smallPackId);
            var packPath = _rootPath.File(packName);
            return await _storage.GetStruct<SmallPackData>(packPath) ?? throw new Exception($"Failed to load '{string.Format(_index.SmallPackName, smallPackId)}'");
        }

        protected async ValueTask<SmallPackData> GetSmallPack_Unsafe(int smallPackId)
        {
            if (_cachedSmallPackId != smallPackId || _cachedSmallPack == null)
            {
                _cachedSmallPack = await GetSmallPack_Unsafe_NoCache(smallPackId);
                _cachedSmallPackId = smallPackId;
            }
                
            return _cachedSmallPack.Value;
        }

        protected async Task<TData> GetElement_Unsafe(int elementId)
        {
            string elementName = string.Format(_index.ElementName, elementId);
            var elementPath = _rootPath.File(elementName);
            var element = await _storage.Get<TData>(elementPath);
            if (element == null)
            {
                throw new Exception("Failed to load data");
            }

            return element;
        }

        public async Task ClearCache()
        {
            await _locker.WaitAsync();
            _cachedSmallPackId = -1;
            _cachedSmallPack = null;
            _cachedBigPackId = -1;
            _cachedBigPack = null;
            _locker.Release();
        }

        public async Task<int> GetCount()
        {
            await _locker.WaitAsync();
            try
            {
                var index = await GetIndex_Unsafe();
                return index.Count;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task<TData?> GetAt(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            
            await _locker.WaitAsync();
            try
            {
                var index = await GetIndex_Unsafe();
                if (id >= index.Count)
                {
                    return null;
                }

                int bigPacksNumber = index.Count / index.BigPackSize;
                int bigPackId = id / index.BigPackSize;
                if (bigPackId < bigPacksNumber)
                {
                    var pack = await GetBigPack_Unsafe(bigPackId);
                    return pack.List[id % index.BigPackSize];
                }
                
                int smallPacksNumber = (index.Count % index.BigPackSize) / index.SmallPackSize;
                int smallPackId = (id % index.BigPackSize) / index.SmallPackSize;
                if (smallPackId < smallPacksNumber)
                {
                    var pack = await GetSmallPack_Unsafe(smallPackId);
                    return pack.List[id % index.SmallPackSize];
                }

                string elementName = string.Format(index.ElementName, id % index.SmallPackSize);
                var elementPath = _rootPath.File(elementName);
                var element = await _storage.Get<TData>(elementPath);
                if (element == null)
                {
                    throw new Exception("Failed to load data");
                }
                return element;
            }
            finally
            {
                _locker.Release();
            }
        }

        public IAsyncEnumerable<IReadOnlyList<TData>> GetAll()
        {
            return GetMany(0);
        }
        
        public async IAsyncEnumerable<IReadOnlyList<TData>> GetMany(int from, int till = -1)
        {
            await _locker.WaitAsync();
            try
            {
                var index = await GetIndex_Unsafe();

                if (index.Count == 0)
                {
                    yield break;
                }

                if (till < 0)
                {
                    till = index.Count - 1;
                }

                if (from < 0 || from > till || till >= index.Count)
                {
                    throw new Exception();
                }
                
                int bigPacksNumber = index.Count / index.BigPackSize;
                {
                    int fromBigPackId = from / index.BigPackSize;
                    int tillBigPackId = Math.Min(till / index.BigPackSize, bigPacksNumber - 1);

                    List<(int bigPackId, Task<BigPackData> loadTask)> tasks = new();
                    for (int bigPackId = fromBigPackId; bigPackId <= tillBigPackId; ++bigPackId)
                    {
                        int _bigPackId = bigPackId;
                        tasks.Add((bigPackId, GetBigPack_Unsafe_NoCache(_bigPackId)));
                    }
                    
                    foreach (var (bigPackId, loadTask) in tasks)
                    {
                        var pack = await loadTask;
                        int a, b;
                        if (bigPackId == fromBigPackId && bigPackId == tillBigPackId)
                        {
                            a = from % index.BigPackSize;
                            b = Math.Min(till, bigPacksNumber * index.BigPackSize - 1) % index.BigPackSize;
                        }
                        else if (bigPackId == fromBigPackId)
                        {
                            a = from % index.BigPackSize;
                            b = index.BigPackSize - 1;
                        }
                        else if (bigPackId == tillBigPackId)
                        {
                            a = 0;
                            b = Math.Min(till, bigPacksNumber * index.BigPackSize - 1) % index.BigPackSize;
                        }
                        else
                        {
                            a = 0;
                            b = index.BigPackSize - 1;
                        }

                        if (b - a == index.BigPackSize - 1)
                        {
                            yield return pack.List;
                        }
                        else
                        {
                            TData[] range = new TData[b - a + 1];
                            for (int i = a; i <= b; ++i)
                            {
                                range[i - a] = pack.List[i];
                            }

                            yield return range;
                        }
                    }
                }
                
                from = Math.Max(from, bigPacksNumber * index.BigPackSize);
                if (till < from)
                {
                    yield break;
                }
                from %= index.BigPackSize;
                till %= index.BigPackSize;

                int smallPacksNumber = (index.Count % index.BigPackSize) / index.SmallPackSize;
                {
                    int fromSmallPackId = from / index.SmallPackSize;
                    int tillSmallPackId = Math.Min(till / index.SmallPackSize, smallPacksNumber - 1);
                    
                    List<(int smallPackId, Task<SmallPackData> loadTask)> tasks = new();
                    for (int smallPackId = fromSmallPackId; smallPackId <= tillSmallPackId; ++smallPackId)
                    {
                        int _smallPackId = smallPackId;
                        tasks.Add((_smallPackId, GetSmallPack_Unsafe_NoCache(_smallPackId)));
                    }

                    foreach (var (smallPackId, loadTask) in tasks)
                    {
                        var pack = await loadTask;
                        int a, b;
                        if (smallPackId == fromSmallPackId && smallPackId == tillSmallPackId)
                        {
                            a = from % index.SmallPackSize;
                            b = Math.Min(till, smallPacksNumber * index.SmallPackSize - 1) % index.SmallPackSize;
                        }
                        else if (smallPackId == fromSmallPackId)
                        {
                            a = from % index.SmallPackSize;
                            b = index.SmallPackSize - 1;
                        }
                        else if (smallPackId == tillSmallPackId)
                        {
                            a = 0;
                            b = Math.Min(till, smallPacksNumber * index.SmallPackSize - 1) % index.SmallPackSize;
                        }
                        else
                        {
                            a = 0;
                            b = index.SmallPackSize - 1;
                        }

                        if (b - a == index.SmallPackSize - 1)
                        {
                            yield return pack.List;
                        }
                        else
                        {
                            TData[] range = new TData[b - a + 1];
                            for (int i = a; i <= b; ++i)
                            {
                                range[i - a] = pack.List[i];
                            }

                            yield return range;
                        }
                    }
                }   
                
                from = Math.Max(from, smallPacksNumber * index.SmallPackSize);
                if (till < from)
                {
                    yield break;
                }
                from %= index.SmallPackSize;
                till %= index.SmallPackSize;

                {
                    int count = till - from + 1;
                    
                    Task<TData>[] tasks = new Task<TData>[count];
                    for (int i = from; i <= till; ++i)
                    {
                        int _i = i;
                        tasks[i - from] = GetElement_Unsafe(_i);
                    }
                    
                    TData[] range = new TData[count];
                    for (int i = 0; i < count; ++i)
                    {
                        range[i] = await tasks[i];
                    }
                    yield return range;
                }
            }
            finally
            {
                _locker.Release();
            }
        }

        protected struct IndexData() : IVersionedDataStruct
        {
            public int BigPackSize = 0;
            public int SmallPackSize = 0;
            public string BigPackName = "";
            public string SmallPackName = "";
            public string ElementName = "";
            
            public int Count = 0;

            public IndexData(int bigPackSize, int smallPackSize,
                string bigPackName = "big-pack-{0:000}", string smallPackName = "small-pack-{0:000}",
                string elementName = "element-{0:00000}")
                :this()
            {
                BigPackSize = bigPackSize * smallPackSize;
                SmallPackSize = smallPackSize;
                BigPackName = bigPackName;
                SmallPackName = smallPackName;
                ElementName = elementName;
            }

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref BigPackSize);
                serializer.Add(ref SmallPackSize);
                serializer.Add(ref BigPackName, () => throw new Exception());
                serializer.Add(ref SmallPackName, () => throw new Exception());
                serializer.Add(ref ElementName, () => throw new Exception());
                serializer.Add(ref Count);
            }

            public byte Version => 0;
        }
        
        protected struct SmallPackData() : IDataStruct
        {
            public TData[] List = [];
            
            public SmallPackData(TData[] list)
                : this()
            {
                List = list;
            }
            
            public void Serialize(ISerializer serializer)
            {
                serializer.AddArray(ref List, 
                    () => throw new Exception(),
                    (ISerializer s, ref TData value) => s.AddClass(ref value, 
                        () => throw new Exception()));
            }
        }
        
        protected struct BigPackData() : IDataStruct
        {
            public TData[] List = [];
            
            public BigPackData(TData[] list)
                : this()
            {
                List = list;
            }
            
            public void Serialize(ISerializer serializer)
            {
                serializer.AddArray(ref List, 
                    () => throw new Exception(),
                    (ISerializer s, ref TData value) => s.AddClass(ref value, 
                        () => throw new Exception()));
            }
        }
    }
    
    public class BigChainStorage<TData> : BigReadOnlyChainStorage<TData>, IChainStorage<TData>
        where TData : class, IDataStruct
    {
        private readonly IKeyValueStorage _storage;
        
        private readonly List<Task> _cleanupTasks = new List<Task>();

        public bool MaintainCleanStorage { get; set; } = false;

        public static BigChainStorage<TData> ConstructNew(IKeyValueStorage storage, DirPath path, int bigPackSize = 100, int smallPackSize = 100)
        {
            return new(storage, path, new IndexData(bigPackSize, smallPackSize));
        }
        
        public static async Task<BigChainStorage<TData>> ConstructNewAndFlush(IKeyValueStorage storage, DirPath path, int bigPackSize = 100, int smallPackSize = 100)
        {
            var chain = new BigChainStorage<TData>(storage, path, new IndexData(bigPackSize, smallPackSize));
            if (!await chain.IsValid())
            {
                await chain.SetIndex_Unsafe();
                return chain;
            }
            throw new InvalidOperationException("Cannot create new chain");
        }
        
        public static BigChainStorage<TData> LoadFrom(IKeyValueStorage storage, DirPath path)
        {
            return new(storage, path, null);
        }

        public static async Task<BigChainStorage<TData>> LoadOrConstruct(IKeyValueStorage storage, DirPath path, int bigPackSize = 100, int smallPackSize = 100)
        {
            var chain = LoadFrom(storage, path);
            if (!await chain.IsValid())
            {
                chain = ConstructNew(storage, path, bigPackSize, smallPackSize);
            }
            return chain;
        }

        private BigChainStorage(IKeyValueStorage storage, DirPath rootPath, IndexData? index)
            :base(storage, rootPath, false)
        {
            _storage = storage;
            if (index != null)
            {
                _index = index.Value;
                _hasIndex = true;
            }
        }
        
        private async Task SetIndex_Unsafe()
        {
            if (!_hasIndex)
            {
                throw new Exception();
            }
            await _storage.SetVersionedStruct(_rootPath.File(_indexName), _index);
        }

        private Task SetBigPack_Unsafe(int bigPackId, BigPackData bigPackData)
        {
            string bigPackName = string.Format(_index.BigPackName, bigPackId);
            return _storage.SetStruct(_rootPath.File(bigPackName), bigPackData);
        }
        
        private Task SetSmallPack_Unsafe(int smallPackId, SmallPackData smallPackData)
        {
            string smallPackName = string.Format(_index.SmallPackName, smallPackId);
            return _storage.SetStruct(_rootPath.File(smallPackName), smallPackData);
        }

        private Task SetElement_Unsafe(int elementId, TData elementData)
        {
            string elementName = string.Format(_index.ElementName, elementId);
            return _storage.Set(_rootPath.File(elementName), elementData);
        }

        public async Task<int> Append(TData data)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            
            await _locker.WaitAsync();
            try
            {
                var index = await GetIndex_Unsafe();

                int bigPacksCount = index.Count / index.BigPackSize;
                int smallPacksCount = (index.Count % index.BigPackSize) / index.SmallPackSize;
                int elementsCount = index.Count % index.SmallPackSize;

                int cleaningDepth;

                if (elementsCount + 1 < index.SmallPackSize)
                {
                    // write element data
                    await SetElement_Unsafe(elementsCount, data);
                    await IncAndUpdateIndex();
                    
                    return _index.Count - 1;
                }
                
                // if (elementsCount + 1 == index.SmallPackSize)
                SmallPackData smallPackData = new SmallPackData(new TData[index.SmallPackSize]);
                {
                    Task<TData?>[] _elementsGetTasks = new Task<TData?>[index.SmallPackSize - 1];

                    for (int i = 0; i < index.SmallPackSize - 1; ++i)
                    {
                        string elementName = string.Format(index.ElementName, i);
                        var path = _rootPath.File(elementName);
                        _elementsGetTasks[i] = _storage.Get<TData>(path);
                    }
                    for (int i = 0; i < index.SmallPackSize - 1; ++i)
                    {
                        var element = await _elementsGetTasks[i];
                        _elementsGetTasks[i] = null!;
                        smallPackData.List[i] = element ?? throw new Exception($"Failed to load {string.Format(index.ElementName, i)}");
                    }
                    
                    smallPackData.List[index.SmallPackSize - 1] = data;
                    cleaningDepth = 1;
                }

                if (smallPacksCount + 1 < index.BigPackSize / index.SmallPackSize)
                {
                    // write element data
                    await SetSmallPack_Unsafe(smallPacksCount, smallPackData);
                    await IncAndUpdateIndex();

                    await Clean(cleaningDepth);
                    return _index.Count - 1;
                }
                
                // if (smallPacksCount + 1 == index.BigPackSize / index.SmallPackSize)
                BigPackData bigPackData = new BigPackData(new TData[index.BigPackSize]);
                {
                    int size = index.BigPackSize / index.SmallPackSize;
                    
                    Task<SmallPackData?>[] _smallPacksGetTasks = new Task<SmallPackData?>[size - 1];
                    for (int i = 0; i < size - 1; ++i)
                    {
                        string smallPackName = string.Format(index.SmallPackName, i);
                        var path = _rootPath.File(smallPackName);
                        _smallPacksGetTasks[i] = _storage.GetStruct<SmallPackData>(path);
                    }
                    for (int i = 0; i < size - 1; ++i)
                    {
                        var smallPack = (await _smallPacksGetTasks[i])!.Value;
                        _smallPacksGetTasks[i] = null!;
                        Array.Copy(smallPack.List, 0, 
                            bigPackData.List, i * index.SmallPackSize, index.SmallPackSize);
                    }
                    smallPackData.List[index.SmallPackSize - 1] = data;
                    Array.Copy(smallPackData.List, 0, 
                        bigPackData.List, (size - 1) * index.SmallPackSize, index.SmallPackSize);
                    cleaningDepth = 2;
                }
                
                // write big pack data
                await SetBigPack_Unsafe(bigPacksCount, bigPackData);
                await IncAndUpdateIndex();

                await Clean(cleaningDepth);
                return _index.Count - 1;

                async Task IncAndUpdateIndex()
                {
                    _index.Count += 1;
                    try
                    {
                        await SetIndex_Unsafe();
                    }
                    catch (Exception)
                    {
                        _hasIndex = false;
                        throw;
                    }
                }

                async Task Clean(int depth)
                {
                    _cleanupTasks.Clear();
                    bool maintainCleanStorage = MaintainCleanStorage;
                    if (depth >= 1)
                    {
                        if (maintainCleanStorage)
                        {
                            for (int i = 0; i < index.SmallPackSize - 1; ++i)
                            {
                                string elementName = string.Format(index.ElementName, i);
                                var path = _rootPath.File(elementName);
                                _cleanupTasks.Add(_storage.Erase(path));
                            }
                        }
                    }

                    if (depth >= 2)
                    {
                        // clear small packs cache
                        _cachedSmallPack = null;
                        _cachedSmallPackId = -1;
                        if (maintainCleanStorage)
                        {
                            int smallPacksCountInBigPack = index.BigPackSize / index.SmallPackSize;
                            for (int i = 0; i < smallPacksCountInBigPack - 1; ++i)
                            {
                                string smallPackName = string.Format(index.SmallPackName, i);
                                var path = _rootPath.File(smallPackName);
                                _cleanupTasks.Add(_storage.Erase(path));
                            }
                        }
                    }

                    if (maintainCleanStorage)
                    {
                        await Task.WhenAll(_cleanupTasks);
                    }
                }
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task RewriteData(bool includeIndex, bool includePacks,
            bool includeElements, Action<int>? progress = null)
        {
            await _locker.WaitAsync();
            try
            {
                var index = await GetIndex_Unsafe();
                if (includeIndex)
                {
                    await SetIndex_Unsafe();
                }

                int processed = 0;

                int totalBigPacks = index.Count / index.BigPackSize;
                int totalSmallPacks = index.Count % index.BigPackSize / index.SmallPackSize;
                
                if (includePacks)
                {
                    for (int i = 0; i < totalBigPacks; ++i)
                    {
                        var pack = await GetBigPack_Unsafe(i);
                        await SetBigPack_Unsafe(i, pack);
                        progress?.Invoke(processed += index.BigPackSize);
                    }
                    for (int i = 0; i < totalSmallPacks; ++i)
                    {
                        var pack = await GetSmallPack_Unsafe(i);
                        await SetSmallPack_Unsafe(i, pack);
                        progress?.Invoke(processed += index.SmallPackSize);
                    }
                }

                if (includeElements)
                {
                    int fromId = totalBigPacks * index.BigPackSize + totalSmallPacks * index.SmallPackSize;
                    for (int i = fromId; i < index.Count; ++i)
                    {
                        int id = i - fromId;
                        var element = await GetElement_Unsafe(id);
                        await SetElement_Unsafe(id, element);
                        progress?.Invoke(processed += 1);
                    }
                }
            }
            finally
            {
                _locker.Release();
            }
        }
    }
}