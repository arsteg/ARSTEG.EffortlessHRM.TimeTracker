using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Models
{
    public class Project
    {

        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<Project> getProjects()
        {
            List<Project> projects = new List<Project>();
            projects.Add(new Project() { Id = 1, Name = "Albiware" });
            projects.Add(new Project() { Id = 2, Name = "Grandprix" });
            projects.Add(new Project() { Id = 3, Name = "Grand" });
            return projects;
        }

        public List<Project> getProjectsv1()
        {
            List<Project> projects = new List<Project>();
            projects.Add(new Project() { Id = 1, Name = "Albiware" });
            projects.Add(new Project() { Id = 2, Name = "Grandprix" });
            return projects;
        }
    }

    public class ProjectTask
    {

        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _projectId;
        public int ProjectId
        {
            get { return _projectId; }
            set { _projectId = value; }
        }

        public List<ProjectTask> getTasks()
        {
            List<ProjectTask> projectTasks = new List<ProjectTask>();
            projectTasks.Add(new ProjectTask() { Id = 1, ProjectId = 1, Name = "Albi task 1" });
            projectTasks.Add(new ProjectTask() { Id = 2, ProjectId = 1, Name = "Albi task 2" });
            projectTasks.Add(new ProjectTask() { Id = 3, ProjectId = 1, Name = "Albi task 3" });
            projectTasks.Add(new ProjectTask() { Id = 4, ProjectId = 2, Name = "grand prix task 1" });
            projectTasks.Add(new ProjectTask() { Id = 5, ProjectId = 2, Name = "grand prix task 2" });
            projectTasks.Add(new ProjectTask() { Id = 6, ProjectId = 3, Name = "grand task 1" });
            projectTasks.Add(new ProjectTask() { Id = 7, ProjectId = 3, Name = "grand task 2" });
            projectTasks.Add(new ProjectTask() { Id = 8, ProjectId = 4, Name = "new grand task 1" });
            return projectTasks;
        }

        public List<ProjectTask> getTaskByProjectId(int projectId)
        {
            List<ProjectTask> taskList = new List<ProjectTask>();
            foreach (ProjectTask projectTask in getTasks())
            {
                if (projectTask.ProjectId == projectId)
                {
                    taskList.Add(new ProjectTask() { Id = projectTask.Id, Name = projectTask.Name, ProjectId = projectTask.ProjectId });
                }
            }
            return taskList;
        }
    }
}
