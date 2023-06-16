using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TimeTracker.Models
{
    public class TimeLogBase
    {
        public string _id { get; set; }
        public string user { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime date { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string filePath { get; set; }
        public int keysPressed { get; set; }
        public int clicks { get; set; }
        public string url { get; set; }
        public int scrolls { get; set; }
    }
    public class TimeLog : TimeLogBase
    {
        public string task { get; set; }
        public DateTime? StartDate { get; set; }
        public string fileString { get; set; }
        public string project { get; set; }
        public string machineId { get; set; }
        public bool makeThisDeviceActive { get; set; }
        public string message { get; set; }
    }
    public class CurrentWeekTotalTime
    {
        public string user { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
    public class ErrorLog
    {
        public string _id { get; set; }
        public string id { get; set; }
        public string error { get; set; }
        public string details { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime updatedOn { get; set; }
        public string status { get; set; }
    }

    public class ProjectRequest
    {
        public string userId { get; set; }
        public DateTime? startDate { get; set; }
        public string projectName { get; set; }
        public DateTime? endDate { get; set; }
        public string notes { get; set; }
        public string estimatedTime { get; set; }
    }

    public class CreateTaskRequest
    {
        public string taskName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime startTime { get; set; }
        public string description { get; set; }
        public string comment { get; set; }
        public string priority { get; set; }
        public string project { get; set; }
        public string[] taskUsers { get; set; }
        public List<TaskAttachment> taskAttachments { get; set; }
        public string title { get; set; }
        public string status { get; set; } 
    }

    public class TaskAttachment
    {
        public string attachmentType { get; set; }
        public string attachmentName { get; set; }
        public string attachmentSize { get; set; }
        public string file { get; set; }
    }

    public class TaskUser
    {
        public string user { get; set; }
    }

    public class TaskRequest
    {
        public string userId { get; set; }
        public string projectId { get; set; }
    }


    public class LiveImageRequest
    {
        public string fileString { get; set; }
    }

    public class LiveImageResponse
    {
        public string status { get; set; }
        public string data { get; set; }
    }
}
