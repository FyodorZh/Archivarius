using System;

namespace Archivarius.Storage
{
    public abstract class Path : IEquatable<Path>, IComparable<Path>
    {
        protected readonly DirPath? _parent;
        private readonly string _path;

        public string FullName => _path;

        public string Name { get; }

        public bool IsDirectory { get; }

        protected Path(DirPath? parent, string name, bool isDirectory)
        {
            _parent = parent;
            Name = name;
            _path = parent == null ? "" : (parent + "/" + name);
            IsDirectory = isDirectory;
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

        public int CompareTo(Path other)
        {
            return String.Compare(_path, other._path, StringComparison.Ordinal);
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
}