using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace AppLocker
{
    public static class AdminActionHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName,
            ushort wLanguage, IntPtr lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        /// <summary>
        /// 替换 exe 文件的图标
        /// </summary>
        public static void SetExeIcon(string exePath, Icon icon)
        {
            string tempIconFile = Path.GetTempFileName() + ".ico";
            using (FileStream fs = new FileStream(tempIconFile, FileMode.Create))
                icon.Save(fs);

            byte[] iconData = File.ReadAllBytes(tempIconFile);
            IntPtr hUpdate = BeginUpdateResource(exePath, false);
            if (hUpdate == IntPtr.Zero) throw new Exception("无法修改可执行文件资源。");

            if (!UpdateResource(hUpdate, (IntPtr)14, (IntPtr)1, 0,
                    Marshal.UnsafeAddrOfPinnedArrayElement(iconData, 0), (uint)iconData.Length))
                throw new Exception("UpdateResource 失败");

            if (!EndUpdateResource(hUpdate, false))
                throw new Exception("EndUpdateResource 失败");

            File.Delete(tempIconFile);
        }
    }
}