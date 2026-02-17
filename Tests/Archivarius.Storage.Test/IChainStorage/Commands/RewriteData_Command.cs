using System.Threading.Tasks;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class RewriteData_Command<TData> : ChainStorageTestCommand<TData, bool>
        where TData : class, IDataStruct
    {
        private readonly bool _includeIndex;
        private readonly bool _includePacks;
        private readonly bool _includeElements;
        
        public RewriteData_Command(bool includeIndex, bool includePacks, bool includeElements)
        {
            _includeIndex = includeIndex;
            _includePacks = includePacks;
            _includeElements = includeElements;
        }
        
        protected override async Task<bool> InvokeOnSubject(ChainStorageWrapper<TData> subject)
        {
            await subject.Storage.RewriteData(_includeIndex, _includePacks, _includeElements);
            return true;
        }
    }
}