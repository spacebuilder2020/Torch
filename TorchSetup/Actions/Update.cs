using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace TorchSetup.Actions
{
    [Verb("update", HelpText = "Update an existing Torch install.")]
    public class Update : ActionBase
    {
        [Option("dry-run", HelpText = "Check for updates but don't perform the update.")]
        public bool DryRun { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync()
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