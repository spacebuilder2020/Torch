using CommandLine;

namespace TorchSetup.Actions
{
    public abstract class VerbBase
    {
        [Option('j', "json", Default = false, HelpText = "Return output as JSON.")]
        public bool ReturnJson { get; set; }
        
        public abstract void Execute();
    }
}