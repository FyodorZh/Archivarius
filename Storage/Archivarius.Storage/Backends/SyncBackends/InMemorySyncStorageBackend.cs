using System;
using System.Collections.Generic;
using System.IO;

namespace Archivarius.Storage
{
    public class InMemorySyncStorageBackend : ISyncStorageBackend
    {
        private readonly DirNode _root = new(null, "");
        
        public event Action<Exception>? OnError;

        public bool ThrowExceptions { get; set; } = true;

        public bool Read<TParam>(FilePath path, TParam param, Action<Stream, TParam> reader)
        {
            try
            {
                var fileNode = FindFileNode(path);

                if (fileNode == null)
                {
                    return false;
                }
                
                fileNode.Stream.Position = 0;
                reader(fileNode.Stream, param);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public bool IsExists(FilePath path)
        {
            try
            {
                var fileNode = FindFileNode(path);
                return fileNode != null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public IReadOnlyList<FilePath> GetSubPaths(DirPath path)
        {
            try
            {
                var dirNode = FindDirNode(path);
                if (dirNode == null)
                {
                    return [];
                }
                
                var list = new List<FilePath>();
                Traverse(list, path, dirNode);
                list.Sort();
                return list;

                void Traverse(List<FilePath> files, DirPath nodePath, DirNode node)
                {
                    foreach (var child in node.Children)
                    {
                        if (child is FileNode file)
                        {
                            files.Add(nodePath.File(file.Name));
                            continue;
                        }
                        Traverse(files, nodePath.Dir(child.Name), (DirNode)child);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return [];
            }
        }

        public bool Write<TParam>(FilePath path, TParam param, Action<Stream, TParam> writer)
        {
            try
            {
                var fileNode = FindOrCreateFileNode(path);

                if (fileNode == null)
                {
                    return false;
                }

                fileNode.Stream.SetLength(0);
                writer(fileNode.Stream, param);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        public bool Erase(FilePath path)
        {
            try
            {
                var fileNode = FindFileNode(path);
                var parent = fileNode?.Parent;
                if (parent != null)
                {
                    if (parent.RemoveChild(fileNode!.Name))
                    {
                        CleanUp(parent);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                if (ThrowExceptions)
                    throw;
                return false;
            }
        }

        private void CleanUp(DirNode dir)
        {
            while (dir.Children.Count == 0)
            {
                var p = dir.Parent;
                if (p == null)
                {
                    break;
                }

                p.RemoveChild(dir.Name);
                dir = p;
            }
        }
        
        private DirNode? FindDirNode(DirPath path)
        {
            if (path.Parent != null)
            {
                DirNode? parentNode = FindDirNode(path.Parent);
                if (parentNode == null)
                {
                    return null;
                }
                
                var child = parentNode.FindChild(path.Name);
                if (child is DirNode dir)
                {
                    return dir;
                }
                return null;
            }

            return _root;
        }

        private FileNode? FindFileNode(FilePath path)
        {
            var dirNode = FindDirNode(path.Parent);
            if (dirNode != null)
            {
                var child = dirNode.FindChild(path.Name);
                return child as FileNode;
            }

            return null;
        }
        
        private FileNode? FindOrCreateFileNode(FilePath path)
        {
            var dirNode = FindOrCreateDirNode(path.Parent);
            if (dirNode == null)
            {
                return null;
            }

            var child = dirNode.FindChild(path.Name);
            if (child == null)
            {
                return dirNode.AddFile(path.Name);
            }

            if (child is FileNode file)
            {
                return file;
            }

            return null;
        }
        
        private DirNode? FindOrCreateDirNode(DirPath path)
        {
            if (path.Parent != null)
            {
                DirNode? parentNode = FindOrCreateDirNode(path.Parent);
                if (parentNode == null)
                {
                    return null;
                }
                
                var child = parentNode.FindChild(path.Name);
                if (child == null)
                {
                    return parentNode.AddDirectory(path.Name);
                }

                if (child is DirNode dir)
                {
                    return dir;
                }

                return null;
            }

            return _root;
        }

        private abstract class Node
        {
            public DirNode? Parent { get; }
            public string Name { get; }

            protected Node(DirNode? parent, string name)
            {
                Name = name;
                Parent = parent;
            }
        }

        private class DirNode : Node
        {
            private readonly Dictionary<string, Node> _children = new();

            public Dictionary<string, Node>.ValueCollection Children => _children.Values;

            public DirNode(DirNode? parent, string name) 
                : base(parent, name)
            {
            }

            public bool RemoveChild(string name)
            {
                return _children.Remove(name);
            }

            public Node? FindChild(string name)
            {
                _children.TryGetValue(name, out var node);
                return node;
            }

            public DirNode AddDirectory(string name)
            {
                var node = new DirNode(this, name);
                _children.Add(name, node);
                return node;
            }
            
            public FileNode AddFile(string name)
            {
                var node = new FileNode(this, name);
                _children.Add(name, node);
                return node;
            }
            
            public override string ToString()
            {
                if (Parent == null)
                {
                    return "/";
                }
                return Parent + Name + "/";
            }
        }
        
        private class FileNode : Node
        {
            public readonly MemoryStream Stream = new();

            public FileNode(DirNode? parent, string name) 
                : base(parent, name)
            {
            }
            
            public override string ToString()
            {
                if (Parent == null)
                {
                    return "/";
                }
                return Parent + Name;
            }
        }
    }
}