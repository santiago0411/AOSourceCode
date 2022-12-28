using System.Diagnostics;
using UnityDebug = UnityEngine.Debug;

namespace AOClient.Core.Utils
{
    public static class DebugLogger
    {
        private enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("AO_LOG_DEBUG")]
        public static void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }
        
        [Conditional("UNITY_EDITOR")]
        [Conditional("AO_LOG_DEBUG")]
        [Conditional("AO_LOG_INFO")]
        public static void Info(string message)
        {
            Log(message, LogLevel.Info);
        }
        
        [Conditional("UNITY_EDITOR")]
        [Conditional("AO_LOG_DEBUG")]
        [Conditional("AO_LOG_INFO")]
        [Conditional("AO_LOG_WARN")]
        public static void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }
        
        [Conditional("UNITY_EDITOR")]
        [Conditional("AO_LOG_DEBUG")]
        [Conditional("AO_LOG_INFO")]
        [Conditional("AO_LOG_WARN")]
        [Conditional("AO_LOG_ERROR")]
        public static void Error(string message)
        {
            Log(message, LogLevel.Error);
        }

        private static void Log(string message, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    UnityDebug.Log(message);
                    break;
                case LogLevel.Info:
                    UnityDebug.Log(message);
                    break;
                case LogLevel.Warn:
                    UnityDebug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    UnityDebug.LogError(message);
                    break;
            }
        }
    }
}