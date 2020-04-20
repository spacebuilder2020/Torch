using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace TorchSetup.Actions
{
    [Verb("install", HelpText = "Install a Torch server.")]
    internal class InstallAction : ActionBase
    {
        [Option("branch", Default = "master", HelpText = "The Torch branch to install.")]
        public string Branch { get; set; }
        
        [Option("version", Default = "latest", HelpText = "A specific version to install.")]
        public string Version { get; set; }
        
        [Option("gamebranch", Default = "public", HelpText = "A specific game branch to install.")]
        public string GameBranch { get; set; }
        
        [Option('w', "workers", Default = 8, HelpText = "The number of workers to use to download from Steam.")]
        public int WorkerCount { get; set; }
        
        public override async Task ExecuteAsync()
        {
            Console.WriteLine("Starting install:");
            Console.WriteLine($"  Path:  {Path}");
            Console.WriteLine($"  SEDS:  branch '{GameBranch}'");
            Console.WriteLine($"  Torch: branch '{Branch}', version '{Version}'");
            
            if (!SetupInstallDir(Path, SkipPrompts))
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
            
            await InstallUtils.InstallSeDedicatedAsync(Path, GameBranch, WorkerCount);
            await InstallUtils.InstallTorchAsync(Path, Branch, Version);
            
            var updateInfo = new UpdateConfig
            {
                Branch = Branch,
                Version = Version,
                GameBranch = GameBranch
            };
            File.WriteAllText(updateInfoPath, JsonConvert.SerializeObject(updateInfo));
            
            Console.WriteLine("The installation completed successfully.");
        }
        
        private static bool SetupInstallDir(string path, bool skipPrompts)
        {
            var exeName = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            
            if (!Directory.Exists(path))
            {
                if (skipPrompts || Program.Confirm("The specified directory does not exist. Create?"))
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
                if (!(skipPrompts || Program.Confirm("The specified directory is not empty. Continue?")))
                {
                    Console.WriteLine("Error - the installation directory is not empty.");
                    return false;
                }
            }

            return true;
        }
    }
}