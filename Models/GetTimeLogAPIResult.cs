using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Models
{
    public class GetTimeLogAPIResult
    {
        public string status { get; set; }
        public TimeLogList data { get; set; }
    }

    public class TimeLogList
    {
        public List<TimeLog> timeLogs { get; set; }
    }

}
