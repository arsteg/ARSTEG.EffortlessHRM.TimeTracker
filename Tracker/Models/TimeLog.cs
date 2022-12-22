using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TimeTracker.Models
{
    public class TimeLog
    {
        public string _id { get; set; }
        public string user { get; set; }
        public string task { get; set; }
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime date { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string filePath { get; set; }
        public string fileString { get; set; }
        public int keysPressed { get; set; }
        public int clicks { get; set; }
        public string url { get; set; }
    }
    public class CurrentWeekTotalTime
    {
        public string user { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

}
