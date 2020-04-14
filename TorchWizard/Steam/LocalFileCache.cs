using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TorchWizard.Steam
{
    [ProtoContract]
    public class LocalFileCache
    {
        [ProtoMember(0)]
        public Dictionary<string, FileInfo> Files { get; set; }

        public LocalFileCache()
        {
            Files = new Dictionary<string, FileInfo>();
        }
    }

    [ProtoContract]
    public class FileInfo
    {
        [ProtoMember(0)]
        public byte[] Hash { get; set; }
        
        [ProtoMember(1)]
        public DateTime LastModified { get; set; }

        public FileInfo()
        {
            Hash = null;
            LastModified = DateTime.MinValue;
        }
    }
}