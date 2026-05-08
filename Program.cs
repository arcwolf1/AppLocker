using System;
using System.Drawing;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 检查是否以管理员身份执行特定操作
            if (args.Length > 0 && args[0] == "/admin" && args.Length >= 2)
            {
                RunAdminAction(args[1]);
                return;
            }

            // 以下为原有正常启动逻辑（不重复定义 myPath 等）
            string myPath = Application.ExecutablePath;
            bool isFaked = IsFakedProgram(myPath);

            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                if (isFaked)
                {
                    string rawArgs = GetRawCommandLineArgs();
                    Application.Run(new LauncherForm(myPath, rawArgs));
                }
                else
                {
                    Application.Run(new LauncherForm(args[0], null));
                }
            }
            else
            {
                if (isFaked)
                {
                    Application.Run(new LauncherForm(myPath));
                }
                else
                {
                    Application.Run(new ManagerForm());
                }
            }
        }

        // 判断自身是否为已伪装的程序
        private static bool IsFakedProgram(string exePath)
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
            if (!File.Exists(configPath)) return false;

            var lines = File.ReadAllLines(configPath);
            return lines.Any(line =>
            {
                var parts = line.Split('|');
                return parts.Length >= 1 &&
                       parts[0].Equals(exePath, StringComparison.OrdinalIgnoreCase);
            });
        }

        // 获取原始命令行参数（去掉程序自身路径）
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

        // 以管理员身份执行具体操作
        private static void RunAdminAction(string base64)
        {
            try
            {
                var action = AdminAction.FromBase64(base64);
                Logger.Log($"管理员操作：{action.Action}, 文件：{action.FakePath}");

                if (action.Action == "replace")
                {
                    PerformReplaceAsAdmin(action);
                }
                else if (action.Action == "restore")
                {
                    PerformRestoreAsAdmin(action);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"管理员操作失败：{ex.Message}");
                MessageBox.Show("操作失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void PerformReplaceAsAdmin(AdminAction action)
        {
            // 1. 提取原程序图标
            Icon originalIcon = Icon.ExtractAssociatedIcon(action.IconSourcePath);

            // 2. 删除可能已存在的备份文件
            if (File.Exists(action.BackupPath))
                File.Delete(action.BackupPath);

            // 3. 备份原程序
            File.Move(action.FakePath, action.BackupPath);

            // 4. 复制自身到目标位置
            string myExe = Application.ExecutablePath;
            File.Copy(myExe, action.FakePath, overwrite: true);

            // 5. 替换图标
            if (originalIcon != null)
            {
                AdminActionHelper.SetExeIcon(action.FakePath, originalIcon);
                originalIcon.Dispose();
            }

            // 6. 写入目标目录配置文件已在提权前完成，此处无需处理
        }

        private static void PerformRestoreAsAdmin(AdminAction action)
        {
            // 删除伪装 exe
            if (File.Exists(action.FakePath))
                File.Delete(action.FakePath);

            // 将备份改回原文件名
            if (File.Exists(action.BackupPath))
                File.Move(action.BackupPath, action.FakePath);
        }
    }
}