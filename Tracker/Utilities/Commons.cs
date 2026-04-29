using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Utilities
{
    internal static class Commons
    {
        // Shared HttpClient for URL accessibility checks
        private static readonly HttpClient _sharedHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

        public static bool IsMacOS()
        {
            return Environment.OSVersion.Platform == PlatformID.MacOSX ||
                   (Environment.OSVersion.Platform == PlatformID.Unix &&
                    System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                        System.Runtime.InteropServices.OSPlatform.OSX));
        }

        public static bool IsLinux()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix && !IsMacOS();
        }

        public static async Task<bool> CheckUrlAccessibility(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    HttpResponseMessage response = await _sharedHttpClient.SendAsync(request).ConfigureAwait(false);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                // Timeout
                return false;
            }
        }
    }
}
