using System;
using System.Diagnostics;
using System.IO;

namespace AppLocker
{
    public static class Logger
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "applocker.log");
        private static readonly object LockObj = new object();

        public static void Log(string message)
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            
            // 输出到调试窗口（仅在 DEBUG 模式下有效）
            Debug.WriteLine(logLine);
            
            // 写入文件
            try
            {
                lock (LockObj)
                {
                    File.AppendAllText(LogFile, logLine + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}