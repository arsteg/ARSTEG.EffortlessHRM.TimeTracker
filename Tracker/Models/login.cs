using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TimeTracker.Models
{
    public class Login
    {
        private string _id;

        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string password { get; set; }

        // Handle both "_id" (from MongoDB) and "id" (from API)
        [JsonProperty("_id")]
        public string MongoId { set { if (!string.IsNullOrEmpty(value)) _id = value; } }

        [JsonProperty("id")]
        public string id
        {
            get => _id;
            set { if (!string.IsNullOrEmpty(value)) _id = value; }
        }

        public Company company { get; set; }
    }

    public class Company
    {
        private string _companyId;

        // Handle both "_id" and "id" for company
        [JsonProperty("_id")]
        public string MongoId { set { if (!string.IsNullOrEmpty(value)) _companyId = value; } }

        [JsonProperty("id")]
        public string id
        {
            get => _companyId;
            set { if (!string.IsNullOrEmpty(value)) _companyId = value; }
        }

        public string companyName { get; set; }
    }
}
