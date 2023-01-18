using DocumentFormat.OpenXml.Drawing.Charts;
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
        public ProjectTaskList data { get; set; }
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
}
