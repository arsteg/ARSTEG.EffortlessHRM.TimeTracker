using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Models
{
    public class ProductivityModel
    {
        public string _id { get; set; }
        public string icon { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public bool? isApproved { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string updatedBy { get; set; }
        public string company { get; set; }
    }
}
