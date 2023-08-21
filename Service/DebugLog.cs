using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IoTDiceLoggingService
{
    // ファイルにタイムスタンプ付きで出力する
    public static class DebugLog
    {
        static string filePath;

        public static void SetPath(string filePath)
        {
            DebugLog.filePath = filePath;
        }

        public static void Write(string message)
        {
            WriteLog(filePath, message);
        }

        public static void WriteLog(string filePath, string message)
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                System.IO.Directory.CreateDirectory(fi.DirectoryName);
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, true, Encoding.UTF8))
                {
                    writer.WriteLine(DateTime.Now.ToString() + " " + message);
                }
            }
            catch { }
        }
    }
}
