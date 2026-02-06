using System.Threading.Tasks;

namespace Archivarius.Storage.Test
{
    public interface ITestCommand<in TestSubject>
    {
        Task<bool> ApplyAndCompare(TestSubject subjectToCheck, TestSubject etalonSubject);
    }
}