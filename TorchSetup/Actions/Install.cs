using System;
using System.IO;
using CommandLine;

namespace TorchSetup.Actions
{
    [Verb("install", HelpText = "Install a Torch server.")]
    public class Install : Update
    {


        /// <inheritdoc />
        public override void Execute()
        {
            if (!Directory.Exists(Path))
            {
                if (Program.Confirm("The specified directory does not exist. Create?"))
                    Directory.CreateDirectory(Path);
            }
            
            base.Execute();
            Console.WriteLine($"Install Torch {Branch} version {Version} with game branch {GameBranch} to {Path}");
            Console.WriteLine($"Skip prompts: {SkipPrompts}");
        }
    }
}