using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TimeTrackerX.Utilities
{
    public static class IdleTimeDetector
    {
        private static DateTime _lastInputTime;

        public static void Initialize()
        {
            _lastInputTime = DateTime.UtcNow;
        }

        public static void UpdateLastInputTime()
        {
            _lastInputTime = DateTime.UtcNow;
        }

        public static TimeSpan GetIdleTime()
        {
            return DateTime.UtcNow.Subtract(_lastInputTime);
        }
    }
}
