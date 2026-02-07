using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test
{
    public abstract class ChainStorageTestCommand<TData, TResult> : ITestCommand<IChainStorage<TData>>
        where TData : class, IDataStruct
    {
        protected abstract Task<TResult> InvokeOnSubject(IChainStorage<TData> subject);
        
        public async Task<bool> ApplyAndCompare(IChainStorage<TData> subjectToCheck, IChainStorage<TData> etalonSubject)
        {
            Exception? ex1 = null, ex2 = null;
            TResult? res1 = default, res2 = default;
            try
            {
                res1 = await InvokeOnSubject(subjectToCheck);
            }
            catch (Exception ex)
            {
                ex1 = ex;
            }
            try
            {
                res2 = await InvokeOnSubject(etalonSubject);
            }
            catch (Exception ex)
            {
                ex2 = ex;
            }

            if ((ex1 != null) != (ex2 != null))
            {
                Assert.Fail($"Exceptions are different: {ex1} and {ex2}");
                return false;
            }

            if ((ex1 != null) && (ex2 != null))
            {
                return true;
            }

            if (res1 == null)
            {
                Assert.That(res2, Is.Null);
                return res2 == null;
            }

            bool res = res1.Equals(res2);
            Assert.That(res1, Is.EqualTo(res2));
            return res;
        }
    }
}