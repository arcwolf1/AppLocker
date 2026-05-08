using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AppLocker
{
    static class Program
    {
        private const string ConfigFile = "applocker.dat";

        [STAThread]
        static void Main(string[] args)
        {
            Logger.Log("========== 应用启动 ==========");
            Logger.Log($"执行路径: {Application.ExecutablePath}");
            Logger.Log($"命令行参数: {(args.Length > 0 ? string.Join(" ", args) : "(无)")}");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string myPath = Application.ExecutablePath;
            bool isFaked = IsFakedProgram(myPath);
            Logger.Log($"自身是否伪装: {isFaked}");

            if (args.Length > 0)
            {
                if (isFaked)
                {
                    string rawArgs = GetRawCommandLineArgs();
                    Logger.Log($"伪装模式 + 参数, 透传参数 = {rawArgs}");
                    Application.Run(new LauncherForm(myPath, rawArgs));
                }
                else
                {
                    Logger.Log($"原始启动器 + 参数模式, 目标 = {args[0]}");
                    Application.Run(new LauncherForm(args[0]));
                }
            }
            else
            {
                if (isFaked)
                {
                    Logger.Log("伪装模式, 无参数, 弹出密码框");
                    Application.Run(new LauncherForm(myPath));
                }
                else
                {
                    Logger.Log("管理工具模式");
                    Application.Run(new ManagerForm());
                }
            }
        }

        private static bool IsFakedProgram(string exePath)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
            if (!File.Exists(configPath))
                return false;

            var lines = File.ReadAllLines(configPath);
            return lines.Any(line =>
            {
                var parts = line.Split('|');
                return parts.Length >= 1 &&
                       parts[0].Equals(exePath, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static string GetRawCommandLineArgs()
        {
            string cmdLine = Environment.CommandLine;
            string exePath = Application.ExecutablePath;

            int index = cmdLine.IndexOf(exePath, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                cmdLine = cmdLine.Remove(index, exePath.Length).TrimStart();
            }
            else if (cmdLine.StartsWith("\""))
            {
                int closeQuote = cmdLine.IndexOf('"', 1);
                if (closeQuote > 0)
                    cmdLine = cmdLine.Substring(closeQuote + 1).TrimStart();
            }
            return cmdLine.Trim();
        }
    }
}