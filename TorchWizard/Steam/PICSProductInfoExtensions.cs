using System;
using SteamKit2;
using static SteamKit2.SteamApps.PICSProductInfoCallback;

namespace TorchWizard.Steam
{
    public static class PICSProductInfoExtensions
    {
        public static ulong GetManifestId(this PICSProductInfo info, uint depotId, string branch)
        {
            return info.GetSection(EAppInfoSection.Depots)[depotId.ToString()]["manifests"][branch].AsUnsignedLong();
        }
        
        public static KeyValue GetSection(this PICSProductInfo info, EAppInfoSection section)
        {
            switch (section)
            {
                case EAppInfoSection.Depots:
                    return info.KeyValues["depots"];
                default:
                    throw new NotSupportedException(section.ToString("G"));
            }
        }
    }
}