using System.Collections.Generic;
using Torch.Managers;
using Torch.Plugins;

namespace Torch.Commands
{
    public class ConsoleCommandContext : CommandContext
    {
        public List<TorchChatMessage> Responses = new List<TorchChatMessage>();

        /// <inheritdoc />
        public ConsoleCommandContext(ITorchBase torch, ITorchPlugin plugin, ulong steamIdSender, string rawArgs = null, List<string> args = null)
            : base(torch, plugin, steamIdSender, rawArgs, args) { }

        /// <inheritdoc />
        public override void Respond(string message, string sender = null, string font = null)
        {
            if (sender == "Server")
            {
                sender = null;
                font = null;
            }

            Responses.Add(new TorchChatMessage(sender ?? TorchBase.Instance.Config.ChatName, message, default));
        }
    }
}