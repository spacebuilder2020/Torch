using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace TorchSetup.Actions
{
    [Verb("update", HelpText = "Update an existing Torch install.")]
    internal class UpdateAction : ActionBase
    {
        [Option("branch", Default = null, HelpText = "The Torch branch to install.")]
        public string Branch { get; set; }
        
        [Option("version", Default = null, HelpText = "A specific version to install.")]
        public string Version { get; set; }
        
        [Option("gamebranch", Default = null, HelpText = "A specific game branch to install.")]
        public string GameBranch { get; set; }
        
        [Option('w', "workers", Default = 8, HelpText = "The number of workers to use to download from Steam.")]
        public int WorkerCount { get; set; }
        
        public override async Task ExecuteAsync()
        {
            var updateInfoPath = System.IO.Path.Combine(Path, "updatecfg.json");
            if (!File.Exists(updateInfoPath))
            {
                Console.WriteLine("An existing installation is not detected in this directory. Use the install command instead.");
                return;
            }

            var updateInfo = JsonConvert.DeserializeObject<UpdateConfig>(File.ReadAllText(updateInfoPath));

            if (!string.IsNullOrEmpty(Branch))
                updateInfo.Branch = Branch;

            if (!string.IsNullOrEmpty(Version))
                updateInfo.Version = Version;

            if (!string.IsNullOrEmpty(GameBranch))
                updateInfo.GameBranch = GameBranch;
            
            await InstallUtils.InstallSeDedicatedAsync(Path, GameBranch, WorkerCount);
            await InstallUtils.InstallTorchAsync(Path, Branch, Version);
            
            File.WriteAllText(updateInfoPath, JsonConvert.SerializeObject(updateInfo));
            
            Console.WriteLine("The update completed successfully.");
        }
    }
}