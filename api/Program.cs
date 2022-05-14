using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace api
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]private static extern bool AttachConsole(int dwProcessId);
        [DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll",SetLastError=true)]private static extern uint GetWindowThreadProcessId(IntPtr hWnd,out int lpdwProcessId);
        public static void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
        [STAThread]static void Main(string[]args)
        {
            /* Stdout fix */
            Console.OpenStandardOutput();
            AttachConsole(-1);
            /* Variables */
            string DPDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low/DiscordProtector";
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
            /* Continue if args exist */
            if (args.Length != 0)
            {
                /* Parse first argument */
                switch(args[0])
                {
                    case "--registerinstallation":
                        /* register new installation */
                        if(args.Length == 3)
                        {
                            /* Path */
                            string Path=args[1];
                            var VersionSP = Path.Split("/".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                            var Version = VersionSP[VersionSP.Length-1];
                            var Edition = args[2];

                            /* Open DP data */
                            if (!Directory.Exists(DPDataPath))
                            {
                                Directory.CreateDirectory(DPDataPath);
                            };
                            if (!Directory.Exists($"{DPDataPath}/versions"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/versions");
                            };
                            if (!Directory.Exists($"{DPDataPath}/protections"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/protections");
                            };
                            if (!Directory.Exists($"{DPDataPath}/clientdata"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/clientdata");
                            };
                            if (!File.Exists($"{DPDataPath}/key.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/key.discordprotector","");
                            };

                            /* Hide dir to prevent viruses looking for dir */
                            Process.Start("cmd.exe",$"/C attrib +h +s \"{DPDataPath}\"");

                            /* Generate new data dir */
                            var DataDir = Guid.NewGuid().ToString().Replace("-","");

                            /* Move old install dir */
                            if (File.Exists($"{DPDataPath}/versions/{Version}-{Edition}.discordprotector"))
                            {
                                string FC = File.ReadAllText($"{DPDataPath}/versions/{Version}-{Edition}.discordprotector");
                                if (Directory.Exists($"{DPDataPath}/clientdata/{FC}"))
                                {
                                    Directory.Move($"{DPDataPath}/clientdata/{FC}", $"{DPDataPath}/clientdata/{DataDir}");
                                };
                            };

                            /* Move old default dir */
                            if (Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}"))
                            {
                                Directory.Move($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}",$"{DPDataPath}/clientdata/{DataDir}");
                            };

                            /* Write new data dir */
                            File.WriteAllText($"{DPDataPath}/versions/{Version}-{Edition}.discordprotector",DataDir);

                            /* Write protection level if needed */
                            if (!File.Exists($"{DPDataPath}/protections/{Version}-{Edition}.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/protections/{Version}-{Edition}.discordprotector","1");
                            };

                            /* Wait for everything to be complete */
                            Thread.Sleep(5000);
                        };
                        break;
                    case "--discordupdated":
                        /* re install to new discord version */

                        break;
                    case "--getuserdata":
                        /* get user data for discord version */
                        if (args.Length == 3)
                        {
                            /* Path */
                            string Path = args[1];
                            var VersionSP = Path.Split("/".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                            var Version = VersionSP[VersionSP.Length-1];
                            var Edition = args[2];

                            /* Open DP data */
                            if (!Directory.Exists(DPDataPath))
                            {
                                Directory.CreateDirectory(DPDataPath);
                            };
                            if (!Directory.Exists($"{DPDataPath}/versions"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/versions");
                            };
                            if (!Directory.Exists($"{DPDataPath}/protections"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/protections");
                            };
                            if (!Directory.Exists($"{DPDataPath}/clientdata"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/clientdata");
                            };
                            if (!File.Exists($"{DPDataPath}/key.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/key.discordprotector", "");
                            };

                            /* Hide dir to prevent viruses looking for dir */
                            Process.Start("cmd.exe", $"/C attrib +h +s \"{DPDataPath}\"");

                            /* Read protection level */
                            if (File.Exists($"{DPDataPath}/protections/{Version}-{Edition}.discordprotector"))
                            {
                                switch (File.ReadAllText($"{DPDataPath}/protections/{Version}-{Edition}.discordprotector"))
                                {
                                    case "1":
                                        if (File.Exists($"{DPDataPath}/versions/{Version}-{Edition}.discordprotector"))
                                        {
                                            WriteLine(File.ReadAllText($"{DPDataPath}/versions/{Version}-{Edition}.discordprotector"));
                                        };
                                        break;
                                };
                            };
                        };
                        break;
                };
            };
            Environment.Exit(0);
        }
    };
};