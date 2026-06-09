using System;
using System.IO;

namespace MES.Common
{
    public static class MESLogger
    {
        private static readonly object _lock = new object();

        public static void Info(string module, string action, string detail)
            => Write("INFO", module, action, detail);

        public static void Error(string module, string action, Exception ex)
            => Write("ERROR", module, action, ex?.ToString());

        private static void Write(string level, string module, string action, string detail)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] [{module}] [{action}] {detail}";
            Console.WriteLine(line);
            lock (_lock)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"mes_log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
    }
}
