using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AppLocker
{
    public partial class ManagerForm : Form
    {
        private const string ConfigFile = "applocker.dat";
        private List<ProgramEntry> entries = new List<ProgramEntry>();

        public ManagerForm()
        {
            InitializeComponent();
            this.Text = "应用锁 - 管理工具";
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            LoadEntries();
        }

        // 数据实体
        public class ProgramEntry
        {
            public string FakePath { get; set; }      // 伪装后的 exe 路径（原路径）
            public string PasswordHash { get; set; }
            public string BackupPath { get; set; }    // 真身备份路径（xxx.exe.bak）

            public string DisplayText => $"[已伪装] {Path.GetFileName(FakePath)}";
        }

        private void LoadEntries()
        {
            entries.Clear();
            if (File.Exists(ConfigFile))
            {
                foreach (var line in File.ReadAllLines(ConfigFile))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3)
                    {
                        entries.Add(new ProgramEntry
                        {
                            FakePath = parts[0],
                            PasswordHash = parts[1],
                            BackupPath = parts[2]
                        });
                    }
                }
            }
            RefreshList();
        }

        private void SaveEntries()
        {
            File.WriteAllLines(ConfigFile,
                entries.Select(e => $"{e.FakePath}|{e.PasswordHash}|{e.BackupPath}"));
        }

        private void RefreshList()
        {
            listBoxPrograms.Items.Clear();
            foreach (var entry in entries)
                listBoxPrograms.Items.Add(entry.DisplayText);
        }

        // ---------- 触屏友好的密码输入框（密码隐藏） ----------
        private string ShowPasswordDialog(string title, string prompt)
        {
            using (Form pwdForm = new Form())
            {
                pwdForm.Text = title;
                pwdForm.StartPosition = FormStartPosition.CenterScreen;
                pwdForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                pwdForm.MaximizeBox = false;
                pwdForm.MinimizeBox = false;
                pwdForm.ClientSize = new Size(420, 190);
                pwdForm.Font = new Font("微软雅黑", 12F);

                Label lbl = new Label()
                {
                    Text = prompt,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 50
                };

                TextBox txt = new TextBox()
                {
                    Location = new Point(30, 60),
                    Width = 360,
                    UseSystemPasswordChar = true,  // 密码隐藏
                    Font = new Font("微软雅黑", 12F)
                };

                Button btnOk = new Button()
                {
                    Text = "确定",
                    Size = new Size(100, 45),
                    Location = new Point(80, 120),
                    DialogResult = DialogResult.OK
                };

                Button btnCancel = new Button()
                {
                    Text = "取消",
                    Size = new Size(100, 45),
                    Location = new Point(230, 120),
                    DialogResult = DialogResult.Cancel
                };

                pwdForm.Controls.Add(lbl);
                pwdForm.Controls.Add(txt);
                pwdForm.Controls.Add(btnOk);
                pwdForm.Controls.Add(btnCancel);
                pwdForm.AcceptButton = btnOk;
                pwdForm.CancelButton = btnCancel;

                return pwdForm.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }

        // ---------- 核心功能：原位替换 ----------
       private void btnReplaceExe_Click(object sender, EventArgs e)
{
    using (OpenFileDialog ofd = new OpenFileDialog())
    {
        ofd.Filter = "可执行文件 (*.exe)|*.exe";
        if (ofd.ShowDialog() != DialogResult.OK) return;

        string originalExe = ofd.FileName;
        string backupPath = originalExe + ".bak";

        // 检查是否已在列表中（通过 FakePath 精确匹配）
        if (entries.Any(pe => pe.FakePath.Equals(originalExe, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("该程序已被伪装，请先还原。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 1. 输入密码并确认
        string password = ShowPasswordDialog("设置独立密码",
            $"请为“{Path.GetFileName(originalExe)}”设置密码：");
        if (string.IsNullOrEmpty(password))
        {
            MessageBox.Show("未设置密码，操作取消。", "取消", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string confirmPassword = ShowPasswordDialog("确认密码", "请再次输入密码：");
        if (password != confirmPassword)
        {
            MessageBox.Show("两次输入的密码不一致，操作取消。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 2. 二次确认伪装操作
        if (MessageBox.Show(
            $"即将伪装 {Path.GetFileName(originalExe)}，原文件将重命名为 .bak 备份。\n\n" +
            "之后双击该程序将弹出密码框，输入正确密码才能运行原程序。\n\n确认继续？",
            "确认伪装", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        // 3. 计算密码哈希（已去除首尾空格）
        string hash = PasswordHelper.ComputeSha256Hash(password);
        Logger.Log($"伪装程序: {originalExe} -> {backupPath}, 密码哈希: {hash}");

        // 4. 在提权前，先写入目标目录的配置文件（覆盖写入，避免旧条目干扰）
        string targetDir = Path.GetDirectoryName(originalExe);
        string targetConfigPath = Path.Combine(targetDir, ConfigFile);
        string configLine = $"{originalExe}|{hash}|{backupPath}";
        try
        {
            File.WriteAllText(targetConfigPath, configLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Logger.Log($"写入目标配置文件失败: {ex.Message}");
            MessageBox.Show("无法写入配置文件，请确认权限。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 5. 将条目添加到管理工具自身的列表并保存（使 UI 同步）
        entries.Add(new ProgramEntry
        {
            FakePath = originalExe,
            PasswordHash = hash,
            BackupPath = backupPath
        });
        SaveEntries();
        RefreshList();

        // 6. 提权执行文件重命名和图标替换（管理员操作）
        bool success = RunAsAdmin("replace", originalExe, backupPath, iconSourcePath: originalExe);
        if (success)
        {
            MessageBox.Show("伪装成功！现在双击原程序将弹出密码框。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            // 提权失败时，回滚已添加的条目和配置文件
            entries.RemoveAll(pe => pe.FakePath == originalExe);
            SaveEntries();
            RefreshList();
            try { if (File.Exists(targetConfigPath)) File.Delete(targetConfigPath); } catch { }
            MessageBox.Show("操作被用户取消或需要管理员权限，伪装失败。", "失败",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
        // ---------- 还原被替换的程序 ----------
        private void btnRestoreExe_Click(object sender, EventArgs e)
        {
            if (listBoxPrograms.SelectedItem == null)
            {
                MessageBox.Show("请先在列表中选择一个已伪装程序。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        
            var entry = entries[listBoxPrograms.SelectedIndex];
        
            // === 新增：密码验证 ===
            string password = ShowPasswordDialog("验证密码",
                $"请输入“{Path.GetFileName(entry.FakePath)}”的伪装密码：");
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("操作取消。", "取消", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string inputHash = ComputeSha256Hash(password);
            if (inputHash != entry.PasswordHash)
            {
                MessageBox.Show("密码错误，无法还原。", "验证失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Logger.Log($"尝试还原: {entry.FakePath} -> 备份恢复: {entry.BackupPath}");
            // === 密码验证结束 ===
        
            try
            {
                // 删除伪装 exe
                if (File.Exists(entry.FakePath))
                    File.Delete(entry.FakePath);
        
                // 将备份改回原名
                if (File.Exists(entry.BackupPath))
                    File.Move(entry.BackupPath, entry.FakePath);
        
                entries.RemoveAt(listBoxPrograms.SelectedIndex);
                SaveEntries();
                RefreshList();
        
                Logger.Log($"还原成功: {entry.FakePath}");
                MessageBox.Show("已成功还原原始程序。", "完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Log($"还原失败: {ex.Message}");
                MessageBox.Show("还原失败：" + ex.Message +
                    "\n\n请确保以管理员身份运行，且文件未被占用。",
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- 删除条目（仅限未被替换的） ----------
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxPrograms.SelectedItem == null)
            {
                MessageBox.Show("请先选中一个程序。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var entry = entries[listBoxPrograms.SelectedIndex];
            if (!string.IsNullOrEmpty(entry.BackupPath))
            {
                MessageBox.Show("该程序已被伪装，请用“还原原程序”移除。", "无法直接删除",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            entries.RemoveAt(listBoxPrograms.SelectedIndex);
            SaveEntries();
            RefreshList();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
{
    if (listBoxPrograms.SelectedItem == null)
    {
        MessageBox.Show("请先在列表中选择一个已伪装程序。", "提示",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    var entry = entries[listBoxPrograms.SelectedIndex];

    // 1. 验证旧密码
    string oldPassword = ShowPasswordDialog("验证旧密码",
        $"请输入“{Path.GetFileName(entry.FakePath)}”的当前密码：");
    if (string.IsNullOrEmpty(oldPassword))
    {
        MessageBox.Show("操作取消。", "取消", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    string oldHash = PasswordHelper.ComputeSha256Hash(oldPassword);
    if (oldHash != entry.PasswordHash)
    {
        MessageBox.Show("旧密码错误！", "验证失败",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    // 2. 设置新密码
    string newPassword = ShowPasswordDialog("设置新密码",
        $"请为“{Path.GetFileName(entry.FakePath)}”输入新密码：");
    if (string.IsNullOrEmpty(newPassword))
    {
        MessageBox.Show("未输入新密码，操作取消。", "取消",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    // 可选：要求再次确认新密码
    string confirmPassword = ShowPasswordDialog("确认新密码",
        "请再次输入新密码：");
    if (confirmPassword != newPassword)
    {
        MessageBox.Show("两次输入的密码不一致。", "错误",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    // 3. 计算新哈希并更新
    string newHash = PasswordHelper.ComputeSha256Hash(newPassword);
    entry.PasswordHash = newHash;

    // 4. 更新管理工具自身的配置文件
    SaveEntries();

    // 5. 更新目标目录下的配置文件（用覆盖写入，防止旧条目干扰）
    string targetDir = Path.GetDirectoryName(entry.FakePath);
    string targetConfigPath = Path.Combine(targetDir, "applocker.dat");
    string configLine = $"{entry.FakePath}|{newHash}|{entry.BackupPath}";
    File.WriteAllText(targetConfigPath, configLine + Environment.NewLine);

    Logger.Log($"密码已修改: {entry.FakePath}");

    MessageBox.Show("密码修改成功。", "完成",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
}
        // ---------- 创建快捷方式（备用，适合不替换的场景） ----------
        private void btnCreateShortcut_Click(object sender, EventArgs e)
        {
            if (listBoxPrograms.SelectedItem == null)
            {
                MessageBox.Show("请先选中一个程序。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var entry = entries[listBoxPrograms.SelectedIndex];
            string launcherPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutName = Path.GetFileNameWithoutExtension(entry.FakePath) + ".lnk";

            try
            {
                ShortcutHelper.CreateShortcut(desktopFolder, shortcutName,
                    launcherPath, $"\"{entry.FakePath}\"");
                MessageBox.Show($"已创建桌面快捷方式：{shortcutName}", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建快捷方式失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool RunAsAdmin(string actionType, string fakePath, string backupPath, string passwordHash = "", string iconSourcePath = "")
        {
            var action = new AdminAction
            {
                Action = actionType,
                FakePath = fakePath,
                BackupPath = backupPath,
                PasswordHash = passwordHash,
                IconSourcePath = iconSourcePath
            };
            string args = $"/admin {action.ToBase64()}";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = args,
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process p = Process.Start(psi);
                p.WaitForExit();  // 等待管理员进程完成
                return p.ExitCode == 0;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // 用户拒绝了 UAC 提权
                return false;
            }
        }

        // ---------- 辅助：SHA256 ----------
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

        // ========== Windows API：替换exe图标 ==========
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName,
            ushort wLanguage, IntPtr lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetSystemDirectoryW(StringBuilder lpBuffer, uint uSize);

        private static void SetExeIcon(string exePath, Icon icon)
        {
            // 将图标保存为临时文件
            string tempIconFile = Path.GetTempFileName() + ".ico";
            using (FileStream fs = new FileStream(tempIconFile, FileMode.Create))
            {
                icon.Save(fs);
            }

            // 读取图标文件数据
            byte[] iconData = File.ReadAllBytes(tempIconFile);

            IntPtr hUpdate = BeginUpdateResource(exePath, false);
            if (hUpdate == IntPtr.Zero) throw new Exception("BeginUpdateResource 失败，可能无管理员权限");

            try
            {
                // 替换图标组资源 (RT_GROUP_ICON = 14)
                if (!UpdateResource(hUpdate, (IntPtr)14, (IntPtr)1, 0, Marshal.UnsafeAddrOfPinnedArrayElement(iconData, 0), (uint)iconData.Length))
                    throw new Exception("UpdateResource 失败");
            }
            finally
            {
                if (!EndUpdateResource(hUpdate, false))
                    throw new Exception("EndUpdateResource 失败");

                File.Delete(tempIconFile);
            }
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}