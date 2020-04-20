using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace TorchSetup.Actions
{
    internal abstract class ActionBase
    {
        private bool _quietMode;
        
        // Set true by QuietMode if specified
        [Option('y', "noprompt", HelpText = "Automatically accept all prompts.")]
        public bool SkipPrompts { get; set; } = false;
        
        [Option('q', "quiet", Default = false, HelpText = "Skip prompts and console output.")]
        public bool QuietMode { get => _quietMode; set => SkipPrompts |= (_quietMode = value); }

        [Option('d', "dir", HelpText = "The location of the Torch installation.")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();
        
        public abstract Task ExecuteAsync();
    }
}