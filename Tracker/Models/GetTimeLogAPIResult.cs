using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace TimeTracker.Models
{
    public class BaseResponse
    {
        public HttpStatusCode statusCode { get; set; }
        public string status { get; set; }

        public string message { get; set; }
    }

    public class GetTimeLogAPIResult : BaseResponse
    {
        public List<TimeLogBaseResponse> data { get; set; }
    }

    public class AddTimeLogAPIResult : BaseResponse
    {
        public TimeLog data { get; set; }
    }

    public class AddErrorLogAPIResult : BaseResponse
    {
        public AddErrorLog data { get; set; }
    }

    public class AddErrorLog
    {
        public ErrorLog ErrorLog { get; set; }
    }

    public class GetProjectListAPIResult : BaseResponse
    {
        public ProjectList data { get; set; }
    }

    public class ProjectList
    {
        public List<Project> projectList { get; set; }
    }

    public class Project
    {
        public string _id { get; set; }
        public string projectName { get; set; }
    }

    public class ProjectTaskList
    {
        public List<ProjectTask> taskList { get; set; }
    }

    public class GetTaskListAPIResult : BaseResponse
    {
        public List<TaskList> taskList { get; set; }
        public int taskCount { get; set; }
    }

    public class TaskList
    {
        public string id { get; set; }
        public string taskName { get; set; }
        public string description { get; set; }
        public string status { get; set; }
    }

    public class ProjectTask
    {
        public string _id { get; set; }
        public string taskName { get; set; }
        public string description { get; set; }
        public string status { get; set; }
    }

    public class NewtaskData
    {
        public ProjectTask newTask { get; set; }
    }

    public class NewTaskResult : BaseResponse
    {
        public NewtaskData data { get; set; }
    }

    public class ErrorLogList
    {
        public List<ErrorLog> errorLogList { get; set; }
    }

    public class GetErrorLogResult : BaseResponse
    {
        public ErrorLogList data { get; set; }
    }

    public class ProductivityAppResult : BaseResponse
    {
        public List<ProductivityModel> data { get; set; }
    }

    public class ProductivityAppDeleteResult : BaseResponse
    {
        public ProductivityModel data { get; set; }
    }

    public class ProductivityAppAddResult : ProductivityAppDeleteResult { }

    public class TimeLogBaseResponse
    {
        public string _id { get; set; }
        public User user { get; set; }

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

    public class User
    {
        public string _id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public Company company { get; set; }
        public object role { get; set; }
        public string id { get; set; }
    }

    public class CheckLiveScreenResponse
    {
        public bool Success { get; set; }
    }

    public class EnableBeepSoundResult : BaseResponse
    {
        public bool data { get; set; }
    }

    // UserDevice model representing the MongoDB document
    public class UserDevice
    {
        public string userId { get; set; }
        public string machineId { get; set; }
        public bool isOnline { get; set; }
        public string company { get; set; } // Assuming company is stored as a string (ObjectId as string)
        public string _id { get; set; } // MongoDB document ID
    }

    // Data wrapper for success case
    public class UpdateOnlineStatusData
    {
        public UserDevice userDevice { get; set; }
    }

    // Data wrapper for success case
    public class GetOnlineUsersData
    {
        public List<UserDevice> onlineUsers { get; set; }
    }

    // Response for updateOnlineStatus
    public class UpdateOnlineStatusResult : BaseResponse
    {
        public UpdateOnlineStatusData data { get; set; }
    }

    // Response for getOnlineUsersByCompany
    public class GetOnlineUsersByCompanyResult : BaseResponse
    {
        public GetOnlineUsersData data { get; set; }
    }
}
