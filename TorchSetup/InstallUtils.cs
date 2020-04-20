using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NLog;
using SteamKit2;
using TorchSetup.Steam;
using TorchSetup.WebRequests;

namespace TorchSetup
{
    public static class InstallUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        public static async Task InstallSeDedicatedAsync(string path, string branch, int workerCount)
        {
            Log.Info($"Installing Space Engineers Dedicated Server, branch '{branch}'");
            var steamDownloader = new SteamDownloader(SteamConfiguration.Create(x => { }));
            await steamDownloader.LoginAsync();
            await steamDownloader.InstallAsync(298740, 298741, branch, path, workerCount);
            await steamDownloader.LogoutAsync();
        }

        public static async Task InstallTorchAsync(string path, string branch, string version)
        {
            Job job;
            if (version.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
                job = await JenkinsQuery.Instance.GetLatestVersion(branch);
            else
                throw new NotImplementedException("Installing a specific Torch version is not yet supported.");
            
            Log.Info($"Installing Torch Server, branch '{branch}', version '{job.Version}'");

            var zipPath = Path.Combine(path, "torchserver.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            
            await JenkinsQuery.Instance.DownloadRelease(job, zipPath);
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in zip.Entries)
                {
                    var dest = Path.Combine(path, entry.FullName);
                    entry.ExtractToFile(dest, true);
                }
            }
            File.Delete(zipPath);
        }
    }
}