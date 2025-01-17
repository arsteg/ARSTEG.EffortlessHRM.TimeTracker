using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace TimeTracker.Trace
{
    public class LogManager
    {
        public static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Enable or disable logging for a specific level
        public static void SetLoggingEnabled(string level, bool isEnabled)
        {
            var config = NLog.LogManager.Configuration;

            if (config != null)
            {
                // Find the rule corresponding to the level
                foreach (var rule in config.LoggingRules)
                {
                    if (rule.Levels.Contains(LogLevel.FromString(level)))
                    {
                        if (isEnabled)
                        {
                            rule.EnableLoggingForLevel(LogLevel.FromString(level));
                        }
                        else
                        {
                            rule.DisableLoggingForLevel(LogLevel.FromString(level));
                        }
                    }
                }

                // Apply the updated configuration
                NLog.LogManager.ReconfigExistingLoggers();
            }
            // Update the application settings
            switch (level.ToLower())
            {
                case "info":
                    Properties.Settings.Default.EnableInfoLogging = isEnabled;
                    break;
                case "warn":
                    Properties.Settings.Default.EnableWarnLogging = isEnabled;
                    break;
                case "error":
                    Properties.Settings.Default.EnableErrorLogging = isEnabled;
                    break;
            }
            // Save the settings
            Properties.Settings.Default.Save();
        }
    }
}
