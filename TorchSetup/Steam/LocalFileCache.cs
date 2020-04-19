using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TorchSetup.Steam
{
    [ProtoContract]
    public class LocalFileCache
    {
        [ProtoMember(1)]
        public Dictionary<string, FileInfo> Files { get; set; }

        public LocalFileCache()
        {
            Files = new Dictionary<string, FileInfo>();
        }
    }

    [ProtoContract]
    public class FileInfo
    {
        [ProtoMember(1)]
        public byte[] Hash { get; set; }
        
        [ProtoMember(2)]
        public DateTime LastModified { get; set; }

        public FileInfo()
        {
            Hash = null;
            LastModified = DateTime.MinValue;
        }
    }
}