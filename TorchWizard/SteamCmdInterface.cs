using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorchWizard
{
    public static class SteamCmdInterface
    {
        private const string STEAMCMD_DOWNLOAD_URI = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        private const string STEAMCMD_INSTALL_DIR = @"TorchWizard_steamcmd";
        private const string STEAMCMD_EXE = STEAMCMD_INSTALL_DIR + @"\steamcmd.exe";

        private static StringBuilder sb;
        
        private static Process SteamCmdProcess;
        private static Task ReadLoop;
        static SteamCmdInterface()
        {
            byte[] zipData;
            
            if (!File.Exists(STEAMCMD_EXE))
            {
                Console.WriteLine("Downloading SteamCMD...");

                Directory.CreateDirectory(STEAMCMD_INSTALL_DIR);
                
                using (var client = new WebClient())
                    zipData = client.DownloadData(STEAMCMD_DOWNLOAD_URI);
                
                Console.WriteLine("Unpacking SteamCMD...");
                
                using (var zipStream = new ZipArchive(new MemoryStream(zipData)))
                    zipStream.ExtractToDirectory(STEAMCMD_INSTALL_DIR);
            }
        }
        
        public static void Run(string arguments)
        {
            if (SteamCmdProcess != null)
                throw new InvalidOperationException("SteamCMD is already running!");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = STEAMCMD_EXE,
                UseShellExecute = false,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                //RedirectStandardError = true,
                Arguments = arguments
            };
            
            SteamCmdProcess = Process.Start(startInfo);

            SteamCmdProcess.Exited += SteamCmdProcessOnExited;
            SteamCmdProcess.EnableRaisingEvents = true;

            ReadLoop = Task.Run(StdOutReadLoop);
        }

        private static void SteamCmdProcessOnExited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            process.Exited -= SteamCmdProcessOnExited;
            Console.WriteLine("SteamCMD exited!");
            ReadLoop.Wait();
            SteamCmdProcess = null;
        }

        private static void StdOutReadLoop()
        {
            while (!SteamCmdProcess.HasExited)
            {
                if (SteamCmdProcess.StandardOutput.Peek() > -1)
                {
                    Console.Write(SteamCmdProcess.StandardOutput.ReadToEnd());
                }
                else
                    Thread.Sleep(1);
            }
            
            Console.Write(SteamCmdProcess.StandardOutput.ReadToEnd());
        }
    }
}