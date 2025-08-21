using System;
using System.Collections.Generic;

namespace Archivarius.Storage
{
    public class PathFactory
    {
        private readonly Dictionary<string, DirPath> _directories = new();

        public Path BuildWithCache(string path)
        {
            var pos = path.LastIndexOf('/');
            if (pos == path.Length - 1)
            {
                return GetDir(path.Substring(0, path.Length - 1));
            }

            return new FilePath(GetDir(path.Substring(0, pos)), path.Substring(pos + 1));
        }

        private DirPath GetDir(string path)
        {
            if (!_directories.TryGetValue(path, out var dir))
            {
                dir =  ConstructDir(path);
                _directories[path] = dir;
            }

            return dir;
        }

        private DirPath ConstructDir(string path)
        {
            if (path == "")
            {
                return DirPath.Root;
            }
            
            var pos = path.LastIndexOf('/');
            var parent = GetDir(path.Substring(0, pos));
            return new DirPath(parent, path.Substring(pos + 1));
        }
        
        public static DirPath BuildDir(string path)
        {
            if (path[path.Length - 1] != '/')
            {
                throw new InvalidOperationException();
            }
            
            var list = path.Split('/');
            
            DirPath dir = DirPath.Root;
            for (int i = 1; i < list.Length - 1; ++i)
            {
                dir = new DirPath(dir, list[i]);
            }
            
            return dir;
        }
        
        public static FilePath BuildFile(string path)
        {
            if (path[path.Length - 1] == '/')
            {
                throw new InvalidOperationException();
            }
            
            var list = path.Split('/');
            
            DirPath dir = DirPath.Root;
            for (int i = 1; i < list.Length - 1; ++i)
            {
                dir = new DirPath(dir, list[i]);
            }

            return new FilePath(dir, list[list.Length - 1]);
        }
    }
}