using System;

namespace Archivarius.Storage
{
    public abstract class Path : IEquatable<Path>
    {
        private readonly string _name;
        private readonly string _path;

        public DirPath? Parent { get; }

        public string FullName => _path;

        public string Name => _name;
        
        public abstract bool IsDirectory { get; }

        protected Path(DirPath? parent, string name)
        {
            Parent = parent;
            _name = name;
            _path = (parent?.ToString() ?? "") + name;
        }

        public override string ToString()
        {
            return _path;
        }

        public bool Equals(Path? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _path == other._path;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Path)obj);
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }

        public static bool operator ==(Path? left, Path? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Path? left, Path? right)
        {
            return !Equals(left, right);
        }
    }

    public class FilePath : Path
    {
        public override bool IsDirectory => false;
        
        public FilePath(DirPath parent, string name)
            :base(parent, name)
        {
            if (name.Contains("/"))
            {
                throw new InvalidOperationException();
            }
        }
    }

    public class DirPath : Path
    {
        public static readonly DirPath Root = new DirPath();

        public override bool IsDirectory => true;

        private DirPath()
            : base(null, "/")
        {}

        public DirPath(DirPath parent, string name)
            : base(parent, name + "/")
        {
            if (name.Contains("/"))
            {
                throw new InvalidOperationException();
            }
        }

        public DirPath Dir(string name) => new DirPath(this, name);

        public FilePath File(string name) => new FilePath(this, name);

        public bool ContainsFile(FilePath file)
        {
            return file.FullName.StartsWith(FullName);
        }
    }
}