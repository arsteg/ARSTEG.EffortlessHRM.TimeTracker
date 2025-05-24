using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Utilities
{
    public static class CheckInternetConnectivity
    {
        //Creating the extern function...
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(
            out int Description,
            int ReservedValue
        );

        //Creating a function that uses the API function...
        public static async Task<bool> IsConnectedToInternetAsync()
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };

                using var response = await httpClient.GetAsync("https://www.google.com", HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
