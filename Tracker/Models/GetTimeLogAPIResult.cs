using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Models
{
    public class GetTimeLogAPIResult
    {
        public string status { get; set; }
        public List<TimeLog> data { get; set; }
    }
}
