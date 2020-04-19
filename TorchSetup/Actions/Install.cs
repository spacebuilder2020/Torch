using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using SteamKit2;
using TorchSetup.Steam;
using TorchSetup.WebRequests;

namespace TorchSetup.Actions
{
    [Verb("install", HelpText = "Install a Torch server.")]
    public class Install : ActionBase
    {
        [Option("branch", Default = "master", HelpText = "The Torch branch to install.")]
        public string Branch { get; set; }
        
        [Option("version", Default = "latest", HelpText = "A specific version to install.")]
        public string Version { get; set; }
        
        [Option("gamebranch", Default = "public", HelpText = "A specific game branch to install.")]
        public string GameBranch { get; set; }
        
        private static bool SetupInstallDir(string path)
        {
            var exeName = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            
            if (!Directory.Exists(path))
            {
                if (Program.Confirm("The specified directory does not exist. Create?"))
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    Console.WriteLine("Error - the installation directory does not exist.");
                    return false;
                }
            }
            else if (Directory.EnumerateFileSystemEntries(path).Any(x => !x.Contains(exeName)))
            {
                if (!Program.Confirm("The specified directory is not empty. Continue?"))
                {
                    Console.WriteLine("Error - the installation directory is not empty.");
                    return false;
                }
            }

            return true;
        }
        
        /// <inheritdoc />
        public override async Task ExecuteAsync()
        {
            Console.WriteLine("Starting install:");
            Console.WriteLine($"  Path:  {Path}");
            Console.WriteLine($"  SEDS:  branch '{GameBranch}'");
            Console.WriteLine($"  Torch: branch '{Branch}', version '{Version}'");
            
            if (!SetupInstallDir(Path))
            {
                Console.WriteLine("Installation cannot continue.");
                return;
            }

            var updateInfoPath = System.IO.Path.Combine(Path, "updatecfg.json");
            if (File.Exists(updateInfoPath))
            {
                Console.WriteLine("An existing installation is detected in this directory. Use the update command instead.");
                return;
            }
            Console.WriteLine($"Installing Space Engineers Dedicated Server, branch '{GameBranch}'");
            var steamDownloader = new SteamDownloader(SteamConfiguration.Create(x => { }));
            await steamDownloader.LoginAsync();
            await steamDownloader.InstallAsync(298740, 298741, GameBranch, Path);
            await steamDownloader.LogoutAsync();
            
            Job version;
            if (Version.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
                version = await JenkinsQuery.Instance.GetLatestVersion(Branch);
            else
                throw new NotImplementedException();
            
            Console.WriteLine($"Installing Torch Server, branch '{Branch}', version '{version.Version}'");

            var zipPath = Path + "\\torchserver.zip";
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            
            await JenkinsQuery.Instance.DownloadRelease(version, zipPath);
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in zip.Entries)
                {
                    var dest = System.IO.Path.Combine(Path, entry.FullName);
                    entry.ExtractToFile(dest, true);
                }
            }
            File.Delete(zipPath);
            
            var updateInfo = new UpdateInfo
            {
                Branch = Branch,
                Version = Version,
                GameBranch = GameBranch
            };
            File.WriteAllText(updateInfoPath, JsonConvert.SerializeObject(updateInfo));
            
            Console.WriteLine("The installation completed successfully.");
        }
    }
}