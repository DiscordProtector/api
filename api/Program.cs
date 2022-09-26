using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace api
{
    internal static class Extensions
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    }
    internal static class Program
    {
        [DllImport("kernel32.dll")]private static extern bool AttachConsole(int dwProcessId);
        [DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll",SetLastError=true)]private static extern uint GetWindowThreadProcessId(IntPtr hWnd,out int lpdwProcessId);
        public static void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
        static Process GetParentProcess()
        {
            int iParentPid = 0;
            int iCurrentPid = Process.GetCurrentProcess().Id;
            IntPtr oHnd = CreateToolhelp32Snapshot(2, 0);
            if (oHnd == IntPtr.Zero)
            {
                CloseHandle(oHnd);
                return null;
            }
            PROCESSENTRY32 oProcInfo = new PROCESSENTRY32();
            oProcInfo.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
            if (Process32First(oHnd, ref oProcInfo) == false)
                return null;
            do
            {
                if (iCurrentPid == oProcInfo.th32ProcessID)
                    iParentPid = (int)oProcInfo.th32ParentProcessID;
            }
            while (iParentPid == 0 && Process32Next(oHnd, ref oProcInfo));
            if (iParentPid > 0)
                return Process.GetProcessById(iParentPid);
            else
                return null;
        }
        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    stream.Dispose();
                    return (BitConverter.ToString(hash).Replace("-", "").ToLower());
                };
            };
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
            /* Get caller process */
            var CallerProcess = GetParentProcess();
            var CallerProcessPath = CallerProcess.GetMainModuleFileName();
            /* Create dirs if missing */
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
            if (!Directory.Exists($"{DPDataPath}/hashes"))
            {
                Directory.CreateDirectory($"{DPDataPath}/hashes");
            };
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
                            var Edition = args[2].ToLower();

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
                            if (!Directory.Exists($"{DPDataPath}/hashes"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/hashes");
                            };
                            if (!File.Exists($"{DPDataPath}/key.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/key.discordprotector","");
                            };

                            /* Hide dir to prevent viruses looking for dir */
                            var CPSI = new ProcessStartInfo();
                            CPSI.CreateNoWindow = true;
                            CPSI.WindowStyle = ProcessWindowStyle.Hidden;
                            CPSI.FileName = "cmd.exe";
                            CPSI.Arguments = $"/C attrib +h +s \"{DPDataPath}\"";
                            Process.Start(CPSI);

                            /* Check if mmm (Man in the middle) attack */
                            if (CallerProcessPath != $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\DiscordProtector\\DiscordProtector.exe")
                            {
                                /* Show popup */
                                MessageBox.Show($"An unauthorised attempt to install Discord Protector was made by: {CallerProcessPath}","Discord Protector",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                                CallerProcess.Kill();
                                return;
                            };

                            /* Generate new data dir */
                            var DataDir = Guid.NewGuid().ToString().Replace("-","");

                            /* Move old install dir */
                            if (File.Exists($"{DPDataPath}/versions/{Edition}.discordprotector"))
                            {
                                string FC = File.ReadAllText($"{DPDataPath}/versions/{Edition}.discordprotector");
                                if (Directory.Exists($"{DPDataPath}/clientdata/{FC}"))
                                {
                                    Directory.Move($"{DPDataPath}/clientdata/{FC}", $"{DPDataPath}/clientdata/{DataDir}");
                                };
                            }
                            else
                            {
                                /* Move old default dir */
                                if (Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}"))
                                {
                                    Directory.Move($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}",$"{DPDataPath}/clientdata/{DataDir}");
                                };
                            };

                            /* Delete old default dir for security */
                            if (Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}"))
                            {
                                Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}",true);
                            };
                            
                            /* Create dir (failsafe) */
                            try{
                                Directory.CreateDirectory($"{DPDataPath}/hashes");
                            }catch{};

                            /* Write new hash */
                            try{
                                File.WriteAllText($"{DPDataPath}/hashes/{Edition}.discordprotector",CalculateMD5($"{Path}/{Edition}.exe"));
                            }catch{};

                            /* Write new data dir */
                            File.WriteAllText($"{DPDataPath}/versions/{Edition}.discordprotector",DataDir);

                            /* Write protection level if needed */
                            if (!File.Exists($"{DPDataPath}/protections/{Edition}.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/protections/{Edition}.discordprotector","1");
                            };

                            /* Wait for everything to be complete */
                            Thread.Sleep(5000);
                        };
                        break;
                    case "--uninstallinstall":
                        /* uninstall an old install */
                        if(args.Length == 3)
                        {
                            /* Variables */
                            string Path = args[1];
                            string Edition = args[2];

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
                            if (!Directory.Exists($"{DPDataPath}/hashes"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/hashes");
                            };
                            if (!File.Exists($"{DPDataPath}/key.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/key.discordprotector", "");
                            };

                            /* Hide dir to prevent viruses looking for dir */
                            var CPSI = new ProcessStartInfo();
                            CPSI.CreateNoWindow = true;
                            CPSI.WindowStyle = ProcessWindowStyle.Hidden;
                            CPSI.FileName = "cmd.exe";
                            CPSI.Arguments = $"/C attrib +h +s \"{DPDataPath}\"";
                            Process.Start(CPSI);

                            /* Check if mmm (Man in the middle) attack */
                            if (CallerProcessPath != $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\DiscordProtector\\DiscordProtector.exe")
                            {
                                /* Show popup */
                                MessageBox.Show($"An unauthorised attempt to uninstall Discord Protector was made by: {CallerProcessPath.Replace("\\", "/")}", "Discord Protector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                CallerProcess.Kill();
                                return;
                            };

                            /* Remove files */
                            var DataDir = "";
                            if (File.Exists($"{DPDataPath}/protections/{Edition}.discordprotector"))
                            {
                                File.Delete($"{DPDataPath}/protections/{Edition}.discordprotector");
                            };
                            if (File.Exists($"{DPDataPath}/versions/{Edition}.discordprotector"))
                            {
                                DataDir = File.ReadAllText($"{DPDataPath}/versions/{Edition}.discordprotector");
                                File.Delete($"{DPDataPath}/versions/{Edition}.discordprotector");
                            };
                            if (File.Exists($"{DPDataPath}/hashes/{Edition}.discordprotector"))
                            {
                                File.Delete($"{DPDataPath}/hashes/{Edition}.discordprotector");
                            };

                            /* Move data dir */
                            if(DataDir != "")
                            {
                                /* Delete old default dir for security */
                                if (Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}"))
                                {
                                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Edition}",true);
                                };

                                /* Move to default dir if exists */
                                if (Directory.Exists($"{DPDataPath}/clientdata/{DataDir}"))
                                {
                                    Directory.Delete($"{DPDataPath}/clientdata/{DataDir}",true);
                                };
                            };
                        };
                        break;
                    case "--getuserdata":
                        /* get user data for discord version */
                        if (args.Length == 3)
                        {
                            /* Variables */
                            string Path = args[1];
                            /*var VersionSP = Path.Split("/".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                            var Version = VersionSP[VersionSP.Length-1];*/
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
                            if (!Directory.Exists($"{DPDataPath}/hashes"))
                            {
                                Directory.CreateDirectory($"{DPDataPath}/hashes");
                            };
                            if (!File.Exists($"{DPDataPath}/key.discordprotector"))
                            {
                                File.WriteAllText($"{DPDataPath}/key.discordprotector", "");
                            };
                            if (!File.Exists($"{DPDataPath}/hashes/{Edition}.discordprotector"))
                            {
                                /* Write new hash */
                                try{
                                    File.WriteAllText($"{DPDataPath}/hashes/{Edition}.discordprotector",CalculateMD5($"{Path}/{Edition}.exe"));
                                }catch{};
                            }

                            /* Hide dir to prevent viruses looking for dir */
                            var CPSI = new ProcessStartInfo();
                            CPSI.CreateNoWindow = true;
                            CPSI.WindowStyle = ProcessWindowStyle.Hidden;
                            CPSI.FileName = "cmd.exe";
                            CPSI.Arguments = $"/C attrib +h +s \"{DPDataPath}\"";
                            Process.Start(CPSI);

                            /* Check if mmm (Man in the middle) attack */
                            if (CallerProcessPath.ToLower().Replace("\\","/") != $"{Path.ToLower().Replace("\\","/")}/{Edition.ToLower()}.exe")
                            {
                                /* Show popup */
                                MessageBox.Show($"An unauthorised attempt to read your data was made by: {CallerProcessPath.Replace("\\","/")}","Discord Protector",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                                CallerProcess.Kill();
                                return;
                            };

                            /* Check if process hash matches */
                            if(CalculateMD5($"{Path.ToLower().Replace("\\","/")}/{Edition.ToLower()}.exe") != File.ReadAllText($"{DPDataPath}/hashes/{Edition}.discordprotector"))
                            {
                                /* Show popup */
                                MessageBox.Show($"An unauthorised attempt to read your data was made by a modified version of Discord, You should re-install Discord immediately.","Discord Protector",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                                CallerProcess.Kill();
                                return;
                            };

                            /* Read protection level */
                            if (File.Exists($"{DPDataPath}/protections/{Edition}.discordprotector"))
                            {
                                switch (File.ReadAllText($"{DPDataPath}/protections/{Edition}.discordprotector"))
                                {
                                    case "1":
                                        if (File.Exists($"{DPDataPath}/versions/{Edition}.discordprotector"))
                                        {
                                            WriteLine(File.ReadAllText($"{DPDataPath}/versions/{Edition}.discordprotector"));
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
        /* Structures */
        [StructLayout(LayoutKind.Sequential)] public struct PROCESSENTRY32 { public uint dwSize; public uint cntUsage; public uint th32ProcessID; public IntPtr th32DefaultHeapID; public uint th32ModuleID; public uint cntThreads; public uint th32ParentProcessID; public int pcPriClassBase; public uint dwFlags; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile; };
        /* DLL Imports */
        [DllImport(@"kernel32.dll", SetLastError = true)] static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);
        [DllImport(@"kernel32.dll")] static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        [DllImport(@"kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(IntPtr hObject);
        [DllImport(@"kernel32.dll")] static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
    };
};
