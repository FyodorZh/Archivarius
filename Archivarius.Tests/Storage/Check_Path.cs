using Archivarius.Storage;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_Path
    {
        [Test]
        public void Test()
        {
            Assert.That(DirPath.Root.FullName, Is.EqualTo("/"));
            Assert.That(DirPath.Root.Parent, Is.Null);
            Assert.That(DirPath.Root.Dir("dir").FullName, Is.EqualTo("/dir/"));
            Assert.That(DirPath.Root.File("file").FullName, Is.EqualTo("/file"));
            Assert.That(DirPath.Root.Dir("dir").Parent, Is.EqualTo(DirPath.Root));
            Assert.That(DirPath.Root.Dir("dir").File(DirPath.Root.Dir("nested").File("file")).FullName,
                Is.EqualTo("/dir/nested/file"));
            Assert.That(DirPath.Root.Dir("dir").Dir(DirPath.Root.Dir("nested").Dir("nested2")).FullName,
                Is.EqualTo("/dir/nested/nested2/"));
            Assert.That(PathFactory.BuildDir("/dir/sub/").FullName, Is.EqualTo("/dir/sub/"));
            Assert.That(PathFactory.BuildFile("/dir/file").FullName, Is.EqualTo("/dir/file"));
        }
    }
}