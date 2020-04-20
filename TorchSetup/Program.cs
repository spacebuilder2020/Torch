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
using NLog;
using NLog.Config;
using NLog.Targets;
using SteamKit2;
using TorchSetup.Actions;
using TorchSetup.Steam;

namespace TorchSetup
{
    /// <summary>
    /// Installer and configurator for Torch servers.
    /// </summary>
    internal static class Program
    {
        private static bool IsGUI { get; set; }
        private static Logger Log;

        [STAThread]
        // Not async main because of WPF's STAThread requirement
        public static void Main(string[] args)
        {
            var verbs = new[] {typeof(InstallAction), typeof(UpdateAction), typeof(QueryPluginsAction)};
            Parser.Default.ParseArguments(args, verbs).WithParsed(o =>
            {
                var action = (ActionBase)o;
                ConfigureLogging(action.QuietMode);
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                Log.Debug($"Current dir: {Directory.GetCurrentDirectory()}");
                Log.Debug($"Args: {string.Join(" ", args)}");
                action.ExecuteAsync().Wait();
            });
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal("TorchSetup encountered a fatal error.");
            Log.Fatal(e.ExceptionObject);
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
                var input = Console.ReadLine();
                return input.Equals("y", StringComparison.OrdinalIgnoreCase);
            }
        }
        
        /// <summary>
        /// Sets up NLog to write >= Info to console and >= Debug to "setup.log"
        /// </summary>
        /// <param name="quietMode">If true, disables logging to console.</param>
        private static void ConfigureLogging(bool quietMode)
        {
            var layout = "${time} ${level}| ${message}";
            var config = new LoggingConfiguration();
            var logFile = new FileTarget("logfile")
            {
                FileName = "setup.log", 
                Layout = layout, 
                ReplaceFileContentsOnEachWrite = true
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            if (!quietMode)
            {
                var logConsole = new ColoredConsoleTarget("logconsole")
                {
                    Layout = layout
                };
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);   
            }

            LogManager.Configuration = config;
            Log = LogManager.GetCurrentClassLogger();
        }
    }
}