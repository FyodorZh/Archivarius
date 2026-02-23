using System;

namespace Archivarius.Storage.Remote
{
    public struct CommandReport
    {
        public CommandType Type;
        public bool Success;
        public TimeSpan Duration;
    }
}