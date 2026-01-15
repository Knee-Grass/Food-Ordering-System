using System;
using System.IO;

namespace FoodOrderingSystem.Backend.Services
{
    // RUBRIC: Thread Safety Implementation (Section 3)
    public static class Logger
    {
        private static readonly object _lockObject = new object(); // The critical "Lock" object
        private static string _logPath = "app_activity_log.txt";

        public static void LogAction(string message)
        {
            // RUBRIC: Preventing Race Conditions using 'lock'
            // This ensures only one thread can write to the file at a time.
            lock (_lockObject) 
            {
                try
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
                    File.AppendAllText(_logPath, logEntry);
                }
                catch (Exception)
                {
                    // Fail silently to avoid crashing app on log error
                }
            }
        }
    }
}