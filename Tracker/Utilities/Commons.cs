using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Utilities
{
    internal static class Commons
    {
        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        public static bool IsMacOS()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix &&
                   !IsWindows() && !IsLinux();
        }
        public static bool IsLinux()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix &&
                   !IsWindows() && !IsMacOS();
        }
    }
}
