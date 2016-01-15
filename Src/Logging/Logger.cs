using System;
using log4net;

namespace SqlEtl.Logging
{
    public sealed class Logger
    {
        #region Static Fields

        private static readonly ILog LogInstance = LogManager.GetLogger("log4net");
        private static readonly bool IsDebugEnabled = LogInstance.IsDebugEnabled;
        private static readonly bool IsErrorEnabled = LogInstance.IsErrorEnabled;
        private static readonly bool IsFatalEnabled = LogInstance.IsFatalEnabled;
        private static readonly bool IsInfoEnabled = LogInstance.IsInfoEnabled;
        private static readonly bool IsWarnEnabled = LogInstance.IsWarnEnabled;

        #endregion

        #region Log

        public static void Log(LogLevels level, string message)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    if (IsDebugEnabled)
                    {
                        LogInstance.Debug(message);
                    }
                    break;
                case LogLevels.Error:
                    if (IsErrorEnabled)
                    {
                        LogInstance.Error(message);
                    }
                    break;
                case LogLevels.Fatal:
                    if (IsFatalEnabled)
                    {
                        LogInstance.Fatal(message);
                    }
                    break;
                case LogLevels.Info:
                    if (IsInfoEnabled)
                    {
                        LogInstance.Info(message);
                    }
                    break;
                case LogLevels.Warn:
                    if (IsWarnEnabled)
                    {
                        LogInstance.Warn(message);
                    }
                    break;
                case LogLevels.All:
                    break;
                case LogLevels.Off:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public static void Log(LogLevels level, string message, Exception ex)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    if (IsDebugEnabled)
                    {
                        LogInstance.Debug(message, ex);
                    }
                    break;
                case LogLevels.Error:
                    if (IsErrorEnabled)
                    {
                        LogInstance.Error(message, ex);
                    }
                    break;
                case LogLevels.Fatal:
                    if (IsFatalEnabled)
                    {
                        LogInstance.Fatal(message, ex);
                    }
                    break;
                case LogLevels.Info:
                    if (IsInfoEnabled)
                    {
                        LogInstance.Info(message, ex);
                    }
                    break;
                case LogLevels.Warn:
                    if (IsWarnEnabled)
                    {
                        LogInstance.Warn(message, ex);
                    }
                    break;
                case LogLevels.All:
                    break;
                case LogLevels.Off:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        #endregion
    }
}