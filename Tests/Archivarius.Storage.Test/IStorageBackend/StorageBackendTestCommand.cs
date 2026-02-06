using System;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public abstract class StorageBackendTestCommand<TResult> : ITestCommand<IStorageBackend>
        where TResult : IEquatable<TResult>
    {
        protected abstract Task<TResult> InvokeOnSubject(IStorageBackend subject);
        
        public async Task<bool> ApplyAndCompare(IStorageBackend subjectToCheck, IStorageBackend etalonSubject)
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