using System;

namespace Archivarius.Storage
{
    public class DirPath : Path
    {
        public static readonly DirPath Root = new DirPath();

        public DirPath? Parent => _parent;

        private DirPath()
            : base(null, "", true)
        {}

        public DirPath(DirPath parent, string name)
            : base(parent, name, true)
        {
            if (name.Contains("/") || string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException();
            }
        }

        public DirPath Dir(string name)
        {
            return new DirPath(this, name);
        }

        public FilePath File(string name)
        {
            return new FilePath(this, name);
        }

        public FilePath File(FilePath path)
        {
            return Combine(this, path.Parent).File(path.Name);
        }
        
        public DirPath Dir(DirPath path)
        {
            if (path.Parent == null)
            {
                return this;
            }
            return Combine(this, path);
        }

        private static DirPath Combine(DirPath dir1, DirPath dir2)
        {
            if (dir2._parent != null)
            {
                var dir = Combine(dir1, dir2._parent);
                dir = dir.Dir(dir2.Name);
                return dir;
            }
            return dir1;
        }

        public bool ContainsFile(FilePath file)
        {
            return file.FullName.StartsWith(FullName);
        }
    }
}