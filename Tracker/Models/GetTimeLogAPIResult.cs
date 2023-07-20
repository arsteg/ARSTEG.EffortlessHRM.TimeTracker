using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TimeTracker.Models
{
    public class GetTimeLogAPIResult
    {
        public string status { get; set; }
        public List<TimeLogBaseResponse> data { get; set; }
    }
    public class AddTimeLogAPIResult
    {
        public string status { get; set; }
        public TimeLog data { get; set; }
    }

    public class AddErrorLogAPIResult
    {
        public string status { get; set; }
        public AddErrorLog data { get; set; }
    }

    public class AddErrorLog
    {
        public ErrorLog ErrorLog { get; set; }
    }

    public class GetProjectListAPIResult
    {
        public string status { get; set; }
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

    public class GetTaskListAPIResult
    {
        public string status { get; set; }
        public List<TaskList> data { get; set; }
    }

    public class TaskList
    {
        public string id { get; set; }
        public string taskName { get; set; }
        public string status { get; set; }

    }

    public class ProjectTask
    {
        public string _id { get; set; }
        public string taskName { get; set; }
    }

    public class NewtaskData
    {
        public ProjectTask newTask { get; set; }
    }
    public class NewTaskResult
    {
        public string status { get; set; }
        public NewtaskData data { get; set; }
    }

    public class ErrorLogList
    {
        public List<ErrorLog> errorLogList { get; set; }
    }
    public class GetErrorLogResult
    {
        public string status { get; set; }
        public ErrorLogList data { get; set; }
    }

    public class ProductivityAppResult
    {
        public string status { get; set; }
        public List<ProductivityModel> data { get; set; }
    }

    public class ProductivityAppDeleteResult
    {
        public string status { get; set; }
        public ProductivityModel data { get; set; }
    }

    public class ProductivityAppAddResult: ProductivityAppDeleteResult 
    { 
    }
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
}
