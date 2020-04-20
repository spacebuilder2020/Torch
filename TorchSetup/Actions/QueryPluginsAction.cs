using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;

namespace TorchSetup.Actions
{
    [Verb("queryplugins", HelpText = "Search for plugins on the Torch website.")]
    internal class QueryPluginsAction : ActionBase
    {
        private static readonly WebClient _webClient = new WebClient();
        
        public override async Task ExecuteAsync()
        {
            var json = JObject.Parse(_webClient.DownloadString("https://torchapi.net/api/plugins"));

            foreach (var plugin in json["plugins"])
            {
                Console.WriteLine($"{plugin["id"]} - {plugin["name"]}");
            }
        }
    }
}