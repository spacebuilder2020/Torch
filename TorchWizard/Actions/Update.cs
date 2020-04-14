using System;
using System.IO;
using CommandLine;

namespace TorchWizard.Actions
{
    [Verb("update", HelpText = "Update an existing Torch install.")]
    public class Update : ActionBase
    {
        [Option("dry-run", HelpText = "Check for updates but don't perform the update.")]
        public bool DryRun { get; set; }
        
        [Option("branch", Default = "master", HelpText = "The Torch branch to install.")]
        public string Branch { get; set; }
        
        [Option("version", Default = "latest", HelpText = "A specific version to install.")]
        public string Version { get; set; }
        
        [Option("gamebranch", Default = "public", HelpText = "A specific game branch to install.")]
        public string GameBranch { get; set; }

        /// <inheritdoc />
        public override void Execute()
        {
            if (!Directory.Exists(Path))
            {
                Console.WriteLine("The specified path is not a valid Torch installation.");
            }
            else
            {
                Console.WriteLine("Starting update...");
            }
        }
    }
}