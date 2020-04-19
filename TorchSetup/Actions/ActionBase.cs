using System;
using System.IO;
using CommandLine;

namespace TorchSetup.Actions
{
    public class ActionBase : VerbBase
    {
        [Option('y', "accept", HelpText = "Automatically accept all prompts.")]
        public bool SkipPrompts { get; set; } = false;

        [Option('d', "dir", HelpText = "The location of the Torch installation.")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        /// <inheritdoc />
        public override void Execute() { }
    }
}