using TimeTracker.Models;
using Xunit;

namespace Tracker.Tests.Models
{
    public class ModelsTests
    {
        #region Login Model Tests

        [Fact]
        public void Login_DefaultValues_AreNull()
        {
            var login = new Login();
            Assert.Null(login.email);
            Assert.Null(login.password);
        }

        [Fact]
        public void Login_SetProperties_WorkCorrectly()
        {
            var login = new Login
            {
                email = "test@example.com",
                password = "password123",
                id = "123",
                firstName = "John",
                lastName = "Doe"
            };

            Assert.Equal("test@example.com", login.email);
            Assert.Equal("password123", login.password);
            Assert.Equal("123", login.id);
            Assert.Equal("John", login.firstName);
            Assert.Equal("Doe", login.lastName);
        }

        #endregion

        #region LoginResult Model Tests

        [Fact]
        public void LoginResult_DefaultValues_AreNull()
        {
            var result = new LoginResult();
            Assert.Null(result.status);
            Assert.Null(result.token);
            Assert.Null(result.data);
        }

        [Fact]
        public void LoginResult_SetProperties_WorkCorrectly()
        {
            var result = new LoginResult
            {
                status = "success",
                token = "jwt-token-123",
                data = new LoginresultData { user = new Login { id = "123" } }
            };

            Assert.Equal("success", result.status);
            Assert.Equal("jwt-token-123", result.token);
            Assert.NotNull(result.data);
            Assert.Equal("123", result.data.user.id);
        }

        #endregion

        #region User Model Tests

        [Fact]
        public void User_DefaultValues_AreNull()
        {
            var userModel = new user();
            Assert.Null(userModel._Id);
            Assert.Null(userModel.email);
            Assert.Null(userModel.name);
            Assert.Null(userModel.role);
        }

        [Fact]
        public void User_SetProperties_WorkCorrectly()
        {
            var userModel = new user
            {
                _Id = "user-123",
                email = "test@example.com",
                name = "John Doe",
                role = "admin"
            };

            Assert.Equal("user-123", userModel._Id);
            Assert.Equal("test@example.com", userModel.email);
            Assert.Equal("John Doe", userModel.name);
            Assert.Equal("admin", userModel.role);
        }

        #endregion

        #region Project Model Tests

        [Fact]
        public void Project_DefaultValues_AreNull()
        {
            var project = new Project();
            Assert.Null(project._id);
            Assert.Null(project.projectName);
        }

        [Fact]
        public void Project_SetProperties_WorkCorrectly()
        {
            var project = new Project
            {
                _id = "proj-123",
                projectName = "Test Project"
            };

            Assert.Equal("proj-123", project._id);
            Assert.Equal("Test Project", project.projectName);
        }

        #endregion

        #region ProjectTask Model Tests

        [Fact]
        public void ProjectTask_DefaultValues_AreNull()
        {
            var task = new ProjectTask();
            Assert.Null(task._id);
            Assert.Null(task.taskName);
        }

        [Fact]
        public void ProjectTask_SetProperties_WorkCorrectly()
        {
            var task = new ProjectTask
            {
                _id = "task-123",
                taskName = "Test Task",
                description = "Task description"
            };

            Assert.Equal("task-123", task._id);
            Assert.Equal("Test Task", task.taskName);
            Assert.Equal("Task description", task.description);
        }

        #endregion

        #region TimeLog Model Tests

        [Fact]
        public void TimeLog_DefaultValues()
        {
            var timeLog = new TimeLog();
            Assert.Null(timeLog.user);
        }

        [Fact]
        public void TimeLog_SetProperties_WorkCorrectly()
        {
            var timeLog = new TimeLog
            {
                user = "user-123",
                keysPressed = 100,
                clicks = 50,
                scrolls = 25
            };

            Assert.Equal("user-123", timeLog.user);
            Assert.Equal(100, timeLog.keysPressed);
            Assert.Equal(50, timeLog.clicks);
            Assert.Equal(25, timeLog.scrolls);
        }

        #endregion

        #region ProductivityModel Tests

        [Fact]
        public void ProductivityModel_DefaultValues_AreNull()
        {
            var model = new ProductivityModel();
            Assert.Null(model.key);
            Assert.Null(model.name);
        }

        [Fact]
        public void ProductivityModel_SetProperties_WorkCorrectly()
        {
            var model = new ProductivityModel
            {
                key = "vscode",
                name = "Visual Studio Code",
                isApproved = true,
                status = "approved"
            };

            Assert.Equal("vscode", model.key);
            Assert.Equal("Visual Studio Code", model.name);
            Assert.True(model.isApproved);
            Assert.Equal("approved", model.status);
        }

        #endregion

        #region ErrorLog Model Tests

        [Fact]
        public void ErrorLog_SetProperties_WorkCorrectly()
        {
            var errorLog = new ErrorLog
            {
                error = "An error occurred",
                details = "Stack trace here",
                status = "new"
            };

            Assert.Equal("An error occurred", errorLog.error);
            Assert.Equal("Stack trace here", errorLog.details);
            Assert.Equal("new", errorLog.status);
        }

        #endregion

        #region ApplicationLog Model Tests

        [Fact]
        public void ApplicationLog_SetProperties_WorkCorrectly()
        {
            var log = new ApplicationLog
            {
                appWebsite = "Visual Studio",
                ApplicationTitle = "Project - Visual Studio"
            };

            Assert.Equal("Visual Studio", log.appWebsite);
            Assert.Equal("Project - Visual Studio", log.ApplicationTitle);
        }

        #endregion

        #region CreateUserPreferenceRequest Model Tests

        [Fact]
        public void CreateUserPreferenceRequest_SetProperties_WorkCorrectly()
        {
            var request = new CreateUserPreferenceRequest
            {
                userId = "user-123",
                preferenceKey = "theme",
                preferenceValue = "dark"
            };

            Assert.Equal("user-123", request.userId);
            Assert.Equal("theme", request.preferenceKey);
            Assert.Equal("dark", request.preferenceValue);
        }

        #endregion

        #region LiveImageRequest Model Tests

        [Fact]
        public void LiveImageRequest_SetProperties_WorkCorrectly()
        {
            var request = new LiveImageRequest
            {
                fileString = "base64-encoded-image-data"
            };

            Assert.Equal("base64-encoded-image-data", request.fileString);
        }

        #endregion

        #region TaskRequest Model Tests

        [Fact]
        public void TaskRequest_SetProperties_WorkCorrectly()
        {
            var request = new TaskRequest
            {
                projectId = "project-123",
                userId = "user-123"
            };

            Assert.Equal("project-123", request.projectId);
            Assert.Equal("user-123", request.userId);
        }

        #endregion

        #region ProjectRequest Model Tests

        [Fact]
        public void ProjectRequest_SetProperties_WorkCorrectly()
        {
            var request = new ProjectRequest
            {
                userId = "user-123"
            };

            Assert.Equal("user-123", request.userId);
        }

        #endregion

        #region CreateTaskRequest Model Tests

        [Fact]
        public void CreateTaskRequest_SetProperties_WorkCorrectly()
        {
            var request = new CreateTaskRequest
            {
                taskName = "New Task",
                description = "Task description"
            };

            Assert.Equal("New Task", request.taskName);
            Assert.Equal("Task description", request.description);
        }

        #endregion

        #region BaseResponse Model Tests

        [Fact]
        public void BaseResponse_SetProperties_WorkCorrectly()
        {
            var response = new BaseResponse
            {
                status = "success",
                message = "Operation completed"
            };

            Assert.Equal("success", response.status);
            Assert.Equal("Operation completed", response.message);
        }

        #endregion

        #region UserPreferenceResult Model Tests

        [Fact]
        public void UserPreferenceResult_DefaultValues()
        {
            var result = new UserPreferenceResult();
            Assert.False(result.isBeepSoundEnabled);
            Assert.False(result.isScreenshotNotificationEnabled);
            Assert.False(result.isBlurScreenshot);
            Assert.Equal(0, result.weeklyHoursLimit);
            Assert.Equal(0, result.monthlyHoursLimit);
        }

        [Fact]
        public void UserPreferenceResult_SetProperties_WorkCorrectly()
        {
            var result = new UserPreferenceResult
            {
                isBeepSoundEnabled = true,
                isScreenshotNotificationEnabled = true,
                isBlurScreenshot = true,
                weeklyHoursLimit = 40,
                monthlyHoursLimit = 160
            };

            Assert.True(result.isBeepSoundEnabled);
            Assert.True(result.isScreenshotNotificationEnabled);
            Assert.True(result.isBlurScreenshot);
            Assert.Equal(40, result.weeklyHoursLimit);
            Assert.Equal(160, result.monthlyHoursLimit);
        }

        #endregion

        #region CheckLiveScreenResponse Model Tests

        [Fact]
        public void CheckLiveScreenResponse_DefaultValue_IsFalse()
        {
            var response = new CheckLiveScreenResponse();
            Assert.False(response.Success);
        }

        [Fact]
        public void CheckLiveScreenResponse_SetSuccess_WorksCorrectly()
        {
            var response = new CheckLiveScreenResponse { Success = true };
            Assert.True(response.Success);
        }

        #endregion

        #region UpdateOnlineStatusResult Model Tests

        [Fact]
        public void UpdateOnlineStatusResult_SetProperties_WorkCorrectly()
        {
            var result = new UpdateOnlineStatusResult
            {
                status = "success",
                data = new UpdateOnlineStatusData
                {
                    userDevice = new UserDevice
                    {
                        userId = "user-123",
                        machineId = "machine-456",
                        isOnline = true
                    }
                }
            };

            Assert.Equal("success", result.status);
            Assert.NotNull(result.data);
            Assert.NotNull(result.data.userDevice);
            Assert.Equal("user-123", result.data.userDevice.userId);
            Assert.True(result.data.userDevice.isOnline);
        }

        #endregion

        #region EventData Model Tests

        [Fact]
        public void EventData_SetProperties_WorkCorrectly()
        {
            var eventData = new EventData
            {
                EventName = "TestEvent",
                UserId = "user-123"
            };

            Assert.Equal("TestEvent", eventData.EventName);
            Assert.Equal("user-123", eventData.UserId);
        }

        #endregion

        #region UserPreferencesKey Model Tests

        [Fact]
        public void UserPreferencesKey_HasCorrectDefaultValues()
        {
            var key = new UserPreferencesKey();

            Assert.Equal("Tracker.SelectedProject", key.TrackerSelectedProject);
            Assert.Equal("Tracker.ScreenshotSoundDisabled_explicit", key.ScreenshotSoundDisabled);
            Assert.Equal("Tracker.ScreenshotNotificationDisabled_explicit", key.ScreenshotNotificationDisabled);
            Assert.Equal("Tracker.ScreenshotBlur_explicit", key.ScreenshotBlur);
            Assert.Equal("Tracker.WeeklyHoursLimit_explicit", key.WeeklyHoursLimit);
            Assert.Equal("Tracker.MonthlyHoursLimit_explicit", key.MonthlyHoursLimit);
        }

        #endregion

        #region LoginresultData Model Tests

        [Fact]
        public void LoginresultData_DefaultValue_IsNull()
        {
            var data = new LoginresultData();
            Assert.Null(data.user);
        }

        [Fact]
        public void LoginresultData_SetUser_WorksCorrectly()
        {
            var data = new LoginresultData
            {
                user = new Login { email = "test@example.com", id = "123" }
            };

            Assert.NotNull(data.user);
            Assert.Equal("test@example.com", data.user.email);
            Assert.Equal("123", data.user.id);
        }

        #endregion

        #region Company Model Tests

        [Fact]
        public void Company_SetProperties_WorkCorrectly()
        {
            var company = new Company
            {
                _id = "comp-123",
                companyName = "Test Company",
                id = "comp-123"
            };

            Assert.Equal("comp-123", company._id);
            Assert.Equal("Test Company", company.companyName);
            Assert.Equal("comp-123", company.id);
        }

        #endregion
    }
}
