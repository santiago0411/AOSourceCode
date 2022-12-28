using System;
using log4net;

namespace AO.Core.Logging
{
    public sealed class LoggerAdapter
    {
        private readonly ILog logger;

        public LoggerAdapter(Type type)
        {
            logger = LogManager.GetLogger(type);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Debug<T0>(string message, T0 arg0)
        {
            if (logger.IsDebugEnabled)
                logger.DebugFormat(message, arg0);
        }
        
        public void Debug<T0, T1>(string message, T0 arg0, T1 arg1)
        {
            if (logger.IsDebugEnabled)
                logger.DebugFormat(message, arg0, arg1);
        }
        
        public void Debug<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
        {
            if (logger.IsDebugEnabled)
                logger.DebugFormat(message, arg0, arg1, arg2);
        }
        
        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Info<T0>(string message, T0 arg0)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat(message, arg0);
        }
        
        public void Info<T0, T1>(string message, T0 arg0, T1 arg1)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat(message, arg0, arg1);
        }
        
        public void Info<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat(message, arg0, arg1, arg2);
        }
        
        public void Warn(string message)
        {
            logger.Warn(message);
        }

        public void Warn<T0>(string message, T0 arg0)
        {
            logger.WarnFormat(message, arg0);
        }
        
        public void Warn<T0, T1>(string message, T0 arg0, T1 arg1)
        {
            logger.WarnFormat(message, arg0, arg1);
        }
        
        public void Warn<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
        {
            logger.WarnFormat(message, arg0, arg1, arg2);
        }
        
        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Error<T0>(string message, T0 arg0)
        {
            logger.ErrorFormat(message, arg0);
        }
        
        public void Error<T0, T1>(string message, T0 arg0, T1 arg1)
        {
            logger.ErrorFormat(message, arg0, arg1);
        }
        
        public void Error<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
        {
            logger.ErrorFormat(message, arg0, arg1, arg2);
        }

        public void Error<T0, T1, T2, T3>(string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            logger.ErrorFormat(message, arg0, arg1, arg2, arg3);
        }
    }
}