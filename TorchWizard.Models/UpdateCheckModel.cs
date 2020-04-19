using System.Collections.Generic;

namespace TorchSetup.Models
{
    public class UpdateCheckModel
    {
        public string GameVersion { get; set; }
        
        public string TorchVersion { get; set; }
        
        public Dictionary<string, string> PluginVersions { get; set; }
    }
}