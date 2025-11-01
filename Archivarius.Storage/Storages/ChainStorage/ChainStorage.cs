using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class ReadOnlyChainStorage<TData> : IReadOnlyChainStorage<TData>
        where TData : class, IDataStruct
    {
        private readonly IReadOnlyKeyValueStorage _storage;
        protected readonly DirPath _rootPath;
        
        protected readonly SemaphoreSlim _locker = new(1, 1);

        private readonly bool _noIndexCache;
        protected bool _hasIndex;
        protected IndexData _index;

        protected int _cachedPackId = -1;
        protected PackData? _cachedPack;
        
        public static ReadOnlyChainStorage<TData> LoadFrom(IReadOnlyKeyValueStorage storage, DirPath path, bool noCache = false)
        {
            return new(storage, path, noCache);
        }

        protected ReadOnlyChainStorage(IReadOnlyKeyValueStorage storage, DirPath rootPath, bool noIndexCache)
        {
            _storage = storage;
            _rootPath = rootPath;
            _index = default;
            _hasIndex = false;
            _noIndexCache = noIndexCache;
        }

        public async Task<bool> IsValid()
        {
            var indexFile = _rootPath.File("index");
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
                var index = await _storage.GetVersionedStruct<IndexData>(_rootPath.File("index"));
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

        protected async ValueTask<PackData> GetPack_Unsafe(int packId)
        {
            if (_cachedPackId != packId || _cachedPack == null)
            {
                string packName = string.Format(_index.PackName, packId);
                var packPath = _rootPath.File(packName);
                _cachedPack = await _storage.GetVersionedStruct<PackData>(packPath);
                _cachedPackId = packId;
            }
            
            if (_cachedPack == null)
            {
                throw new Exception("Failed to load data");
            }
                
            return _cachedPack.Value;
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
            _cachedPackId = -1;
            _cachedPack = null;
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

                int packsNumber = index.Count / index.PackSize;
                int packId = id / index.PackSize;
                if (packId < packsNumber)
                {
                    var pack = await GetPack_Unsafe(packId);
                    return pack.List[id % index.PackSize];
                }

                string elementName = string.Format(index.ElementName, id % index.PackSize);
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

                if (till < 0)
                {
                    till = index.Count - 1;
                }

                if (from < 0 || from > till || till >= index.Count)
                {
                    throw new Exception();
                }

                int packsNumber = index.Count / index.PackSize;
                int fromPackId = from / index.PackSize;
                int tillPackId =  Math.Min(till / index.PackSize, packsNumber - 1);
                
                for (int packId = fromPackId; packId <= tillPackId; ++packId)
                {
                    var pack = await GetPack_Unsafe(packId);
                    int a, b;
                    if (packId == fromPackId && packId == tillPackId)
                    {
                        a = from % index.PackSize;
                        b = Math.Min(till, packsNumber * index.PackSize - 1) % index.PackSize;
                    }
                    else if (packId == fromPackId)
                    {
                        a = from % index.PackSize;
                        b = index.PackSize - 1;
                    }
                    else if (packId == tillPackId)
                    {
                        a = 0;
                        b = Math.Min(till, packsNumber * index.PackSize - 1) % index.PackSize;
                    }
                    else
                    {
                        a = 0;
                        b = index.PackSize - 1;
                    }

                    if (b - a == index.PackSize - 1)
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

                if (till >= packsNumber * index.PackSize)
                {
                    int a = 0;
                    if (from >= packsNumber * index.PackSize)
                    {
                        a = from % index.PackSize;
                    }
                    int b = till % index.PackSize;
                    
                    TData[] range = new TData[b - a + 1];
                    for (int i = a; i <= b; ++i)
                    {
                        range[i - a] = await GetElement_Unsafe(i);
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
            public int PackSize = 0;
            public string PackName = "";
            public string ElementName = "";
            
            public int Count = 0;

            public IndexData(int packSize, string packName = "pack-{0:000}", string elementName = "element-{0:00000}")
                :this()
            {
                PackSize = packSize;
                PackName = packName;
                ElementName = elementName;
            }

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref PackSize);
                serializer.Add(ref PackName, () => throw new Exception());
                serializer.Add(ref ElementName, () => throw new Exception());
                serializer.Add(ref Count);
            }

            public byte Version => 0;
        }
        
        protected struct PackData() : IVersionedDataStruct
        {
            public TData[] List = [];
            
            public PackData(TData[] list)
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

            public byte Version => 0;
        }
    }
    
    public class ChainStorage<TData> : ReadOnlyChainStorage<TData>, IChainStorage<TData>
        where TData : class, IDataStruct
    {
        private readonly IKeyValueStorage _storage;

        public static ChainStorage<TData> ConstructNew(IKeyValueStorage storage, DirPath path, int packSize = 1000)
        {
            return new(storage, path, new IndexData(packSize));
        }
        
        public static ChainStorage<TData> LoadFrom(IKeyValueStorage storage, DirPath path)
        {
            return new(storage, path, null);
        }

        public static async Task<ChainStorage<TData>> LoadOrConstruct(IKeyValueStorage storage, DirPath path, int packSize = 1000)
        {
            var chain = LoadFrom(storage, path);
            if (!await chain.IsValid())
            {
                chain = ConstructNew(storage, path, packSize);
            }
            return chain;
        }

        private ChainStorage(IKeyValueStorage storage, DirPath rootPath, IndexData? index)
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
            await _storage.SetVersionedStruct(_rootPath.File("index"), _index);
        }

        private Task SetPack_Unsafe(int packId, PackData packData)
        {
            string packName = string.Format(_index.PackName, packId);
            return _storage.SetVersionedStruct(_rootPath.File(packName), packData);
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

                int elementsCount = index.Count % index.PackSize;
                int packsCount = index.Count / index.PackSize;
                
                if (elementsCount + 1 == index.PackSize)
                {
                    // Prepare pack data
                    TData[] list = new TData[index.PackSize];
                    for (int i = 0; i < index.PackSize - 1; ++i)
                    {
                        string elementName = string.Format(index.ElementName, i);
                        var path = _rootPath.File(elementName);
                        var element = await _storage.Get<TData>(path);
                        list[i] = element ?? throw new Exception($"Failed to load {path}");
                    }
                    list[index.PackSize - 1] = data;
                    
                    // Write pack data
                    await SetPack_Unsafe(packsCount, new PackData(list));

                    // Update index
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
                    
                    // Cleanup elements
                    for (int i = 0; i < index.PackSize - 1; ++i)
                    {
                        string elementName = string.Format(index.ElementName, i);
                        var path = _rootPath.File(elementName);
                        await _storage.Erase(path);
                    }
                }
                else
                {
                    // write element data
                    await SetElement_Unsafe(elementsCount, data);
                    
                    // Update index
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

                return _index.Count - 1;
            }
            finally
            {
                _locker.Release();
            }
        }

        public async Task RewriteData(bool includeIndex, bool includePacks, bool includeElements, Action<int>? progress = null)
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

                int totalPacks = index.Count / index.PackSize;
                
                if (includePacks)
                {
                    for (int i = 0; i < totalPacks; ++i)
                    {
                        var pack = await GetPack_Unsafe(i);
                        await SetPack_Unsafe(i, pack);
                        progress?.Invoke(processed += index.PackSize);
                    }
                }
                if (includeElements)
                {
                    int fromId = totalPacks * index.PackSize;
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