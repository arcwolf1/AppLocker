using System;
using System.Text;
using System.Web.Script.Serialization;

namespace AppLocker
{
    public class AdminAction
    {
        public string Action { get; set; }       // "replace", "restore"
        public string FakePath { get; set; }     // 伪装 exe 路径
        public string BackupPath { get; set; }   // 备份路径
        public string PasswordHash { get; set; } // 可选
        public string IconSourcePath { get; set; } // 提取图标的源文件

        public string ToBase64()
        {
            var json = new JavaScriptSerializer().Serialize(this);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static AdminAction FromBase64(string base64)
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return new JavaScriptSerializer().Deserialize<AdminAction>(json);
        }
    }
}