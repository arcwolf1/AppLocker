using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AppLocker
{
    public partial class LauncherForm : Form
    {
        private readonly string targetPath;
        private readonly string appName;
        private readonly string rawArguments;   // 已经是 string，不是 string[]

        public LauncherForm(string targetPath, string rawArguments = null)
        {
            InitializeComponent();
            // 设置焦点默认在密码框
            this.ActiveControl = txtPassword;
            // 按回车 = 点击“确定”
            this.AcceptButton = btnOK;
            // 按 Esc = 点击“取消”，顺手提升体验
            this.CancelButton = btnCancel;
            this.targetPath = targetPath;
            this.rawArguments = rawArguments;
            this.appName = Path.GetFileNameWithoutExtension(targetPath);
            this.Text = $"需要密码 - {appName}";
            labelHint.Text = $"请输入密码以启动 {appName}";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

      private void btnOK_Click(object sender, EventArgs e)
{
    string backupPath = GetBackupPath();
    Logger.Log($"查找备份路径结果：{backupPath ?? "null"}");

    if (string.IsNullOrEmpty(backupPath))
    {
        Logger.Log("错误：未找到备份路径");
        MessageBox.Show("未找到备份的真实程序，请检查 applocker.dat。", "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
        return;
    }

    if (!File.Exists(backupPath))
    {
        Logger.Log($"错误：备份文件不存在 {backupPath}");
        MessageBox.Show($"备份文件不存在：\n{backupPath}", "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
        return;
    }

    if (!ValidatePassword())
    {
        Logger.Log("密码验证失败");
        MessageBox.Show("密码错误！", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtPassword.Clear();
        txtPassword.Focus();
        return;
    }

    Logger.Log("密码验证通过，准备执行批处理启动");

    string launcherPath = targetPath;
    string originalName = Path.GetFileName(backupPath);               // xxx.exe.bak
    string realName = originalName.Substring(0, originalName.Length - 4); // xxx.exe
    string realPath = Path.Combine(Path.GetDirectoryName(backupPath), realName);
    string tempLauncherName = originalName + ".tmp";
    string tempLauncherPath = Path.Combine(Path.GetDirectoryName(launcherPath), tempLauncherName);
    string workDir = Path.GetDirectoryName(realPath);
    string passThroughArgs = rawArguments ?? "";

    // 最终静默批处理
    string batch = $@"
@echo off
title AppLocker Helper
timeout /t 1 /nobreak >nul
if not exist ""{backupPath}"" exit /b 1
ren ""{launcherPath}"" ""{tempLauncherName}"" 2>nul
if errorlevel 1 exit /b 1
ren ""{backupPath}"" ""{realName}"" 2>nul
if errorlevel 1 goto restore
start """" /D ""{workDir}"" ""{realPath}"" {passThroughArgs}
:wait
tasklist /fi ""imagename eq {realName}"" 2>nul | find /i ""{realName}"" >nul
if not errorlevel 1 (
    timeout /t 2 /nobreak >nul
    goto wait
)
:restore
if exist ""{realPath}"" ren ""{realPath}"" ""{originalName}"" 2>nul
if exist ""{tempLauncherPath}"" ren ""{tempLauncherPath}"" ""{Path.GetFileName(launcherPath)}"" 2>nul
del ""%~f0"" >nul 2>&1
";

    string batchFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bat");
    File.WriteAllText(batchFile, batch);
    Logger.Log($"批处理文件：{batchFile}");
    Logger.Log($"透传参数：{passThroughArgs}");

    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{batchFile}\"",
            WindowStyle = ProcessWindowStyle.Hidden,   // 静默
            UseShellExecute = true
        });
        Logger.Log("已启动批处理，启动器即将退出");
    }
    catch (Exception ex)
    {
        Logger.Log($"启动批处理失败：{ex.Message}");
        MessageBox.Show("无法启动批处理，请检查权限。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    Application.Exit();
}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string GetBackupPath()
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "applocker.dat");
            if (!File.Exists(configFile)) return null;

            foreach (var line in File.ReadAllLines(configFile))
            {
                var parts = line.Split('|');
                if (parts.Length >= 3 && parts[0].Equals(targetPath, StringComparison.OrdinalIgnoreCase))
                    return parts[2];
            }
            return null;
        }

        private bool ValidatePassword()
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "applocker.dat");
            Logger.Log($"验证密码，配置文件路径：{configFile}，目标路径：{targetPath}");

            if (!File.Exists(configFile))
            {
                Logger.Log("密码验证失败：配置文件不存在");
                return false;
            }

            string inputPassword = txtPassword.Text; // 原始输入
            string inputHash = ComputeSha256Hash(inputPassword);
            Logger.Log($"输入密码哈希值：{inputHash}");

            foreach (var line in File.ReadAllLines(configFile))
            {
                var parts = line.Split('|');
                if (parts.Length >= 2 && parts[0].Equals(targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    string storedHash = parts[1];
                    Logger.Log($"找到目标条目，存储的哈希值：{storedHash}");

                    if (storedHash == inputHash)
                    {
                        Logger.Log("密码匹配成功");
                        return true;
                    }
                    else
                    {
                        Logger.Log("密码不匹配，哈希对比失败");
                        // 额外检查：输入是否包含不可见字符
                        Logger.Log($"输入密码长度：{inputPassword.Length}，存储密码哈希对应原始长度未知（仅哈希）");
                        return false;
                    }
                }
            }

            Logger.Log($"未在配置文件中找到匹配路径：{targetPath}");
            return false;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}