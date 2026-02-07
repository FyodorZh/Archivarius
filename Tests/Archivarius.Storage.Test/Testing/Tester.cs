using System.Collections.Generic;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test
{
    public class Tester<TestSubject>
    {
        public async Task<bool> Run(IEnumerable<ITestCommand<TestSubject>> commands, TestSubject subjectToTest, TestSubject etalonSubject)
        {
            // ReSharper disable once NotAccessedVariable
            int dbgId = 0;
            foreach (var command in commands)
            {
                dbgId += 1;
                var result = await command.ApplyAndCompare(subjectToTest, etalonSubject);
                Assert.That(result, Is.True, $"Command #{dbgId} failed");
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}