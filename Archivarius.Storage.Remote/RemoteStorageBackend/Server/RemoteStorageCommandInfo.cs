using System;

namespace Archivarius.Storage.Remote
{
    public struct RemoteStorageCommandInfo
    {
        public readonly string CommandName;
        public bool Success;
        public readonly DateTime StartTime;
        public DateTime EndTime;
        
        public TimeSpan Duration => EndTime - StartTime;
        
        public RemoteStorageCommandInfo(string commandName)
        {
            CommandName = commandName;
            StartTime = DateTime.Now;
            EndTime = StartTime;
            Success = false;
        }

        public void Finish(bool success)
        {
            Success = success;
            EndTime = DateTime.Now;
        }

        public override string ToString()
        {
            return  $"{CommandName}: {(Success ? "OK" : "FAIL")}: DT:{Duration.TotalMilliseconds}ms";
        }
    }
}