using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public static async Task<bool> CheckUrlAccessibility( string url )
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Check if the status code indicates success (2xx)
                    return response.IsSuccessStatusCode;
                }
                catch (HttpRequestException)
                {
                    // An exception occurred, indicating that the URL is not accessible
                    return false;
                }
            }
        }
    }
}
