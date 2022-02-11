using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TimeTracker.Trace
{
    /// <summary>
    /// This class is responsible for logging trace information.
    /// </summary>
    public static class Logger
    {
        #region 

        public static void Error(string message, [CallerMemberName] string callerName = "")
        {
            LogManager.Logger.Error(message);
        }
        public static void Error(Exception ex, [CallerMemberName] string callerName = "")
        {
            LogManager.Logger.Error($"Error occured in method {callerName}\n" + GetExceptionInfo(ex));
        }
        public static void Warning(string message)
        {
            LogManager.Logger.Warn(message);
        }
        public static void Info(string message)
        {
            LogManager.Logger.Info(message);
        }
        #endregion


        /// <summary>
        /// Creates a string containing the exception's message and stack trace
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <returns>A string containing the exception's message and stack trace</returns>
        private static string GetExceptionInfo(Exception ex)
        {
            if (ex == null)
                return string.Empty;

            // Build the exception information string and return it
            var sbInfo = new StringBuilder();
            sbInfo.AppendLine("Exception information:");
            sbInfo.AppendLine("--------------------------------------------------");
            sbInfo.Append("Message: ");
            sbInfo.AppendLine(ex.Message);
            sbInfo.AppendLine("Stack Trace: ");
            sbInfo.AppendLine(ex.StackTrace);
            sbInfo.AppendLine("--------------------------------------------------");

            // If an inner exception is found, generate and append its info
            if (ex.InnerException != null)
            {
                sbInfo.AppendLine("Inner exception");
                sbInfo.AppendLine(GetExceptionInfo(ex.InnerException));
            }
            return sbInfo.ToString();
        }
    }
}
