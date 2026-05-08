using System.Security.Cryptography;
using System.Text;

namespace AppLocker
{
    public static class PasswordHelper
    {
        /// <summary>
        /// 统一密码哈希算法（SHA256，去除首尾空白）
        /// </summary>
        public static string ComputeSha256Hash(string rawData)
        {
            if (rawData == null) rawData = string.Empty;
            rawData = rawData.Trim(); // 强制去除空格，避免因误触导致密码错误

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