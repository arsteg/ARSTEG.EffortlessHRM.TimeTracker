using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTrackerX.Models
{
    public class Login
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string password { get; set; }
        public string id { get; set; }
        public Company company { get; set; }
    }

    public class Company
    {
        public string _id { get; set; }
        public string companyName { get; set; }
        public string id { get; set; }
    }
}
