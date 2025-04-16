using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Models
{
    public class ApplicationLog
    {
        public string appWebsite { get; set; }
        public string ModuleName { get; set; }
        public string ApplicationTitle { get; set; }
        public double TimeSpent { get; set; }
        public DateTime date { get; set; }
        public string type { get; set; }
        public string projectReference { get; set; }
        public string userReference { get; set; }
        public int mouseClicks { get; set; }
        public int keyboardStrokes { get; set; }
        public int scrollingNumber { get; set; }
        public double inactive { get; set; }
        public double total { get; set; }
        public string _id { get; set; }
    }

    public class ApplicationLogResult
    {
        public string status { get; set; }
        public ApplicationLog body { get; set; }
    }
}
