﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace TimeTracker.Models
{
    public class GlobalSetting
    {
        public LoginResult LoginResult { get; set; }
        public Window TimeTracker { get; set; }
        //public const string apiBaseUrl = "https://mysterious-brushlands-61431.herokuapp.com";
        //public const string apiBaseUrl = "http://localhost:8080";
        //public const string apiBaseUrl = "https://arstegeffortlesshrmapi.azurewebsites.net";
        public const string apiBaseUrl = "http://arstegeffortlesshrmapi-dev.us-east-1.elasticbeanstalk.com";


        public const string EmailReceiver = "info@arsteg.com";

        public const string ApiKey = "ec86b9ecfee30654";
        public GlobalSetting()
        {
        }
        public static GlobalSetting Instance { get; } = new GlobalSetting();

        private string ExtractBaseUri(string endpoint)
        {
            var uri = new Uri(endpoint);
            var baseUri = uri.GetLeftPart(System.UriPartial.Authority);

            return baseUri;
        }
    }
}