using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Models
{
    public class LoginResult
    {
        public string status { get; set; }
        public string token { get; set; }
        public LoginresultData data { get; set; }        
    }

    public class LoginresultData
    {
        public Login user { get; set; }
    }

    public class user
    {
        public string _Id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }
}
