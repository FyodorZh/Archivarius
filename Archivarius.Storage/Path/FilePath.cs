using System;

namespace Archivarius.Storage
{
    public class FilePath : Path
    {
        public DirPath Parent => _parent!;
        
        public FilePath(DirPath parent, string name)
            :base(parent, name, false)
        {
            if (name.Contains("/"))
            {
                throw new InvalidOperationException();
            }
        }
    }
}