using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using SteamKit2;
using TorchSetup.Actions;
using TorchSetup.Steam;

namespace TorchSetup
{
    /// <summary>
    /// Installer and configurator for Torch servers.
    /// </summary>
    public static class Program
    {
        public static bool IsGUI { get; set; }

        private static async Task TestSteam()
        {
            var downloader = new SteamDownloader(SteamConfiguration.Create(x => { }));
            await downloader.LoginAsync();
            await downloader.InstallAsync(298740, 298741, "public", "C:\\test_install\\SEDS");
            await downloader.LogoutAsync();
        }
        
        [STAThread]
        // Not async main because of WPF's STAThread requirement
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                IsGUI = true;
                Console.WriteLine("Starting in GUI mode...");
                new MainWindow().ShowDialog();
            }
            else
            {
                var verbs = typeof(Program).Assembly.GetTypes()
                                           .Where(x => x.GetCustomAttribute<VerbAttribute>() != null)
                                           .ToArray();

                Parser.Default.ParseArguments(args, verbs)
                      .WithParsed(HandleParsed);
            }
        }

        private static void HandleParsed(object obj)
        {
            if (obj is VerbBase action)
                action.ExecuteAsync().Wait();
        }

        public static bool Confirm(string message)
        {
            if (IsGUI)
            {
                return MessageBox.Show(message, "Confirmation Dialog", MessageBoxButton.YesNo) 
                       == MessageBoxResult.Yes;
            }
            else
            {
                Console.Write(message);
                Console.Write(" (y/N) ");
                var input = Console.ReadLine().FirstOrDefault();
                return input == 'y' || input == 'Y';
            }
        }
    }
}