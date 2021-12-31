using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Models
{
    public class TimeLog
    {
        public string user { get; set; }
        public string task { get; set; }
        public DateTime date { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }
}
