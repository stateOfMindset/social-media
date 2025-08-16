// Services/FileLogger.cs
using System;
using System.IO;

namespace MiniSocialAPI.Services
{
    public static class FileLogger
    {
        private static readonly string logFile = Path.Combine("Data", "logs.txt");

        public static void Log(string message)
        {
            var logLine = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {message}";
            Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);
            File.AppendAllText(logFile, logLine + Environment.NewLine);
        }
    }
}
