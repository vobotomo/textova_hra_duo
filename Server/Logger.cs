using System;
using System.IO;

namespace Server
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.log");

        public static void Log(string message)
        {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;
            Console.WriteLine(line);
            lock (_lock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine);
            }
        }
    }
}