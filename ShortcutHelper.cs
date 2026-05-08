using System;
using System.IO;
using IWshRuntimeLibrary; // 需添加 COM 引用：Windows Script Host Object Model

namespace AppLocker
{
    public static class ShortcutHelper
    {
        /// <summary>
        /// 创建快捷方式
        /// </summary>
        /// <param name="directory">快捷方式所在目录</param>
        /// <param name="shortcutName">快捷方式文件名（如 "MyApp.lnk"）</param>
        /// <param name="targetPath">目标应用程序路径（此处为 AppLocker.exe 的路径）</param>
        /// <param name="arguments">启动参数（目标程序路径）</param>
        public static void CreateShortcut(string directory, string shortcutName,
            string targetPath, string arguments)
        {
            string shortcutPath = Path.Combine(directory, shortcutName);
            if (System.IO.File.Exists(shortcutPath))       // 使用完全限定名
                System.IO.File.Delete(shortcutPath);       // 避免与 IWshRuntimeLibrary.File 冲突

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.Arguments = arguments;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.IconLocation = targetPath;
            shortcut.Save();
        }
    }
}