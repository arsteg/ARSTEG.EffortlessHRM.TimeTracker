using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Avalonia.Controls;
using static System.Net.WebRequestMethods;

namespace TimeTrackerX.Models
{
    public class GlobalSetting
    {
        public LoginResult LoginResult { get; set; }
        public Window TimeTracker { get; set; }
        public Window LoginView { get; set; }
        public string MachineId { get; set; }

        public Window ProductivityAppsSettings { get; set; }

#if DEBUG
        public const string apiBaseUrl = "https://effortlesshrm-e029cd6a5095.herokuapp.com";

#else
        public const string apiBaseUrl = "https://effortlesshrm-e029cd6a5095.herokuapp.com";
#endif

        //public const string apiBaseUrl = "https://effortlesshrmapi.azurewebsites.net";
        //public const string apiBaseUrl = "https://effortlesshrm-e029cd6a5095.herokuapp.com";

        public const string portalBaseUrl = "https://www.effortlesshrm.com/";

        public const string EmailReceiver = "info@arsteg.com";

        public const string ApiKey = "ec86b9ecfee30654";

        public UserPreferencesKey userPreferenceKey { get; } = new UserPreferencesKey();

        public GlobalSetting() { }

        public static GlobalSetting Instance { get; } = new GlobalSetting();

        private string ExtractBaseUri(string endpoint)
        {
            var uri = new Uri(endpoint);
            var baseUri = uri.GetLeftPart(System.UriPartial.Authority);

            return baseUri;
        }
    }
}
