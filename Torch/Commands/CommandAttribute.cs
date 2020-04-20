using System;
using System.Collections.Generic;
using System.Linq;

namespace Torch.Commands
{
    public class CommandAttribute : Attribute
    {
        /// <summary>
        ///     Provides information about the command. Supports space-delimited hierarchy.
        /// </summary>
        public CommandAttribute(string command, string description = "", string helpText = null)
        {
            var split = command.Split(' ');
            Name = split.Last();
            Description = description;
            HelpText = helpText ?? description;

            Path.AddRange(split);
        }

        public string Name { get; }
        public string Description { get; }
        public string HelpText { get; }
        public List<string> Path { get; } = new List<string>();
    }
}