using System;
using System.Collections.Generic;

namespace Apache.Services
{
    /// <summary>
    /// Serviço de logging.
    /// </summary>
    public class LoggingService
    {
        private static LoggingService _instance;
        private readonly List<string> _logs;
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                        {
                            _instance = new LoggingService();
                        }
                    }
                }
                return _instance;
            }
        }

        private LoggingService()
        {
            _logs = new List<string>();
            _logFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "apache_logs.txt"
            );
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public void LogError(string message, Exception ex = null)
        {
            string errorMessage = ex != null ? $"{message} | Exception: {ex.Message}" : message;
            Log(LogLevel.Error, errorMessage);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        private void Log(LogLevel level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] [{level}] {message}";
            
            lock (_lockObject)
            {
                _logs.Add(logEntry);
            }
            
            System.Diagnostics.Debug.WriteLine(logEntry);

            // Save to file asynchronously without awaiting (fire and forget is intentional here)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SaveLogToFile(logEntry);
#pragma warning restore CS4014
        }

        private async Task SaveLogToFile(string logEntry)
        {
            try
            {
                await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if file write is not possible
            }
        }

        /// <summary>
        /// Gets all logged messages.
        /// </summary>
        public IReadOnlyList<string> GetLogs()
        {
            lock (_lockObject)
            {
                return _logs.AsReadOnly();
            }
        }

        /// <summary>
        /// Clears all logs from memory.
        /// </summary>
        public void ClearLogs()
        {
            lock (_lockObject)
            {
                _logs.Clear();
            }
            LogInfo("Logs cleared");
        }

        private enum LogLevel
        {
            Info,
            Error,
            Warning,
            Debug
        }
    }
}
