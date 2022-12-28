using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using UnityEngine;

#if !UNITY_EDITOR
using System.IO;
#endif

namespace AO.Core.Logging
{
    public static class LoggerConfigurator
    {
        public static void ChangeLogLevel(Level level)
        {
            var log = LogManager.GetLogger(typeof(LoggerConfigurator));
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            log.Info($"Changing log level from {hierarchy.Root.Level} to {level}.");
            hierarchy.Root.Level = level;
            log.Info($"Successfully changed log level to {hierarchy.Root.Level}.");
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureAllLogging()
        {
            var patternLayout = new PatternLayout
            {
                ConversionPattern = "[%date{yyyy-MM-dd HH:mm:ss}] [%level] [%thread] [%logger] - %message%newline"
            };

            patternLayout.ActivateOptions();

            #if UNITY_EDITOR
            var root = ((Hierarchy)LogManager.GetRepository()).Root;
            if (root is IAppenderAttachable attachable)
            {
                var appenders = LogManager.GetRepository().GetAppenders();
                foreach (var appender in appenders)
                    attachable.RemoveAppender(appender);
            }
            
            var fileAppender = new CustomRollingAppender
            {
                AppendToFile = true,
                File = @"D:\ProyectoAO\Logs\ServerAO.log",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "300MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = false,
            };

            fileAppender.ActivateOptions();
            #else
            var fileAppender = new FileAppender
            {
                AppendToFile = true,
                File = Path.Combine(Application.dataPath, "Logs", "ServerAO.log"),
                Layout = patternLayout
            };

            Debug.Log($"Log file: {fileAppender.File}");

            fileAppender.ActivateOptions();
            #endif

            var unityAppender = new UnityAppender
            {
                Layout = new PatternLayout { ConversionPattern = "[%level] - %message" }
            };

            unityAppender.ActivateOptions();
            BasicConfigurator.Configure(unityAppender, fileAppender);
        }

        private class UnityAppender : AppenderSkeleton
        {
            protected override void Append(LoggingEvent loggingEvent)
            {
                // Do not log logs that aren't from this assembly
                if (!loggingEvent.LoggerName.Contains("AO"))
                    return;
                
                string message = loggingEvent.RenderedMessage;
                
                if (Level.Compare(loggingEvent.Level, Level.Error) >= 0)
                {
                    Debug.LogError($"[{loggingEvent.Level.Name}] - {message}");
                }
                else if (Level.Compare(loggingEvent.Level, Level.Warn) >= 0)
                {
                    Debug.LogWarning($"[{loggingEvent.Level.Name}] - {message}");
                }
                else
                {
                    Debug.Log($"[{loggingEvent.Level.Name}] - {message}");
                }
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// This custom Appender avoids logging to a file PlasticSCM logs
        /// </summary>
        private class CustomRollingAppender : RollingFileAppender
        {
            protected override void Append(LoggingEvent loggingEvent)
            {
                if (!loggingEvent.LoggerName.Contains("AO"))
                    return;
                
                base.Append(loggingEvent);
            }

            protected override void Append(LoggingEvent[] loggingEvents)
            {
                foreach (LoggingEvent loggingEvent in loggingEvents)
                {
                    if (!loggingEvent.LoggerName.Contains("AO"))
                        continue;

                    base.Append(loggingEvent);
                }
            }
        }
#endif
    }
}
