using System.Dynamic;
using System.Net;
using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Services.Interfaces;
using Xunit;

namespace Tracker.Tests.Services
{
    public class RESTTests
    {
        private readonly Mock<IHttpProvider> _mockHttpProvider;
        private readonly REST _restService;

        public RESTTests()
        {
            _mockHttpProvider = new Mock<IHttpProvider>();
            _restService = new REST(_mockHttpProvider.Object);
            SetupGlobalSetting();
        }

        private void SetupGlobalSetting()
        {
            GlobalSetting.Instance.LoginResult = new LoginResult
            {
                token = "test-token",
                data = new LoginresultData
                {
                    user = new Login { id = "test-user-id", email = "test@example.com", company = new Company { id = "company-id" } }
                }
            };
        }

        #region CombineUri Tests

        [Fact]
        public void CombineUri_WithMultipleParts_CombinesCorrectly()
        {
            var result = REST.CombineUri("https://api.example.com", "/api/v1/users");
            Assert.Equal("https://api.example.com/api/v1/users", result);
        }

        [Fact]
        public void CombineUri_WithTrailingSlashes_TrimsCorrectly()
        {
            var result = REST.CombineUri("https://api.example.com/", "/api/v1/users/");
            Assert.Equal("https://api.example.com/api/v1/users/", result);
        }

        [Fact]
        public void CombineUri_WithEmptyParts_HandlesGracefully()
        {
            var result = REST.CombineUri("https://api.example.com", "", "api");
            Assert.Equal("https://api.example.com/api", result);
        }

        [Fact]
        public void CombineUri_WithNullParts_HandlesGracefully()
        {
            var result = REST.CombineUri("https://api.example.com", null!, "api");
            Assert.Equal("https://api.example.com/api", result);
        }

        [Fact]
        public void CombineUri_WithEmptyArray_ReturnsEmptyString()
        {
            var result = REST.CombineUri();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CombineUri_WithNullArray_ReturnsEmptyString()
        {
            var result = REST.CombineUri(null!);
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region SignIn Tests

        [Fact]
        public async Task SignIn_WithValidCredentials_ReturnsLoginResult()
        {
            // Arrange
            var login = new Login { email = "test@example.com", password = "password123" };
            var expectedResult = new LoginResult
            {
                status = "success",
                token = "jwt-token",
                data = new LoginresultData { user = new Login { id = "123", email = "test@example.com" } }
            };

            _mockHttpProvider
                .Setup(x => x.PostAsync<LoginResult, Login>(It.IsAny<string>(), login, It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.SignIn(login);

            // Assert
            Assert.Equal("success", result.status);
            Assert.Equal("jwt-token", result.token);
        }

        [Fact]
        public async Task SignIn_CallsCorrectEndpoint()
        {
            // Arrange
            var login = new Login { email = "test@example.com", password = "password123" };

            _mockHttpProvider
                .Setup(x => x.PostAsync<LoginResult, Login>(It.IsAny<string>(), login, It.IsAny<string>()))
                .ReturnsAsync(new LoginResult());

            // Act
            await _restService.SignIn(login);

            // Assert
            _mockHttpProvider.Verify(
                x => x.PostAsync<LoginResult, Login>(
                    It.Is<string>(s => s.Contains("/api/v1/users/login")),
                    login,
                    It.IsAny<string>()),
                Times.Once);
        }

        #endregion

        #region AddTimeLog Tests

        [Fact]
        public async Task AddTimeLog_WithValidTimeLog_ReturnsResult()
        {
            // Arrange
            var timeLog = new TimeLog { user = "test-user-id" };
            var expectedResult = new AddTimeLogAPIResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<AddTimeLogAPIResult, TimeLog>(
                    It.IsAny<string>(), timeLog, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.AddTimeLog(timeLog);

            // Assert
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task AddTimeLog_CallsCorrectEndpoint()
        {
            // Arrange
            var timeLog = new TimeLog();

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<AddTimeLogAPIResult, TimeLog>(
                    It.IsAny<string>(), timeLog, It.IsAny<string>()))
                .ReturnsAsync(new AddTimeLogAPIResult());

            // Act
            await _restService.AddTimeLog(timeLog);

            // Assert
            _mockHttpProvider.Verify(
                x => x.PostWithTokenAsync<AddTimeLogAPIResult, TimeLog>(
                    It.Is<string>(s => s.Contains("/api/v1/timeLogs")),
                    timeLog,
                    "test-token"),
                Times.Once);
        }

        #endregion

        #region GetTimeLogs Tests

        [Fact]
        public async Task GetTimeLogs_ReturnsTimeLogResult()
        {
            // Arrange
            var timeLog = new TimeLog();
            var expectedResult = new GetTimeLogAPIResult
            {
                status = "success",
                data = new List<TimeLogBaseResponse>()
            };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<GetTimeLogAPIResult, TimeLog>(
                    It.IsAny<string>(), timeLog, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetTimeLogs(timeLog);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region GetCurrentWeekTotalTime Tests

        [Fact]
        public async Task GetCurrentWeekTotalTime_ReturnsResult()
        {
            // Arrange
            var request = new CurrentWeekTotalTime { user = "test-user" };
            var expectedResult = new GetTimeLogAPIResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<GetTimeLogAPIResult, CurrentWeekTotalTime>(
                    It.IsAny<string>(), request, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetCurrentWeekTotalTime(request);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region Project and Task Tests

        [Fact]
        public async Task GetProjectListByUserId_ReturnsProjectList()
        {
            // Arrange
            var request = new ProjectRequest { userId = "test-user-id" };
            var expectedResult = new GetProjectListAPIResult
            {
                status = "success",
                data = new ProjectList { projectList = new List<Project> { new Project { projectName = "Test Project" } } }
            };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<GetProjectListAPIResult, ProjectRequest>(
                    It.IsAny<string>(), request, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetProjectListByUserId(request);

            // Assert
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task GetTaskListByProject_ReturnsTaskList()
        {
            // Arrange
            var request = new TaskRequest { projectId = "project-id" };
            var expectedResult = new GetTaskListAPIResult
            {
                status = "success"
            };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<GetTaskListAPIResult, TaskRequest>(
                    It.IsAny<string>(), request, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetTaskListByProject(request);

            // Assert
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task AddNewTask_ReturnsNewTaskResult()
        {
            // Arrange
            var request = new CreateTaskRequest { taskName = "New Task" };
            var expectedResult = new NewTaskResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<NewTaskResult, CreateTaskRequest>(
                    It.IsAny<string>(), request, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.AddNewTask(request);

            // Assert
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task CompleteATask_ReturnsResult()
        {
            // Arrange
            var taskId = "task-123";
            dynamic status = new ExpandoObject();
            status.status = "completed";
            var expectedResult = new NewTaskResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PutWithTokenAsync<NewTaskResult, ExpandoObject>(
                    It.IsAny<string>(), It.IsAny<ExpandoObject>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.CompleteATask(taskId, status);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region Productivity Apps Tests

        [Fact]
        public async Task GetProductivityApps_ReturnsApps()
        {
            // Arrange
            var url = "api/v1/appWebsite/productivity/apps/user-id";
            var expectedResult = new ProductivityAppResult
            {
                data = new List<ProductivityModel> { new ProductivityModel { key = "app1" } }
            };

            _mockHttpProvider
                .Setup(x => x.GetWithTokenAsync<ProductivityAppResult>(
                    It.IsAny<string>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetProductivityApps(url);

            // Assert
            Assert.Single(result.data);
        }

        [Fact]
        public async Task GetProductivityApps_OnException_ReturnsEmptyResult()
        {
            // Arrange
            var url = "api/v1/appWebsite/productivity/apps/user-id";

            _mockHttpProvider
                .Setup(x => x.GetWithTokenAsync<ProductivityAppResult>(
                    It.IsAny<string>(), "test-token"))
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _restService.GetProductivityApps(url);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddProductivityApps_ReturnsResult()
        {
            // Arrange
            var url = "api/v1/appWebsite/productivity";
            var model = new ProductivityModel { key = "newapp" };
            var expectedResult = new ProductivityAppAddResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<ProductivityAppAddResult, ProductivityModel>(
                    It.IsAny<string>(), model, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.AddProductivityApps(url, model);

            // Assert
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task DeleteProductivityApp_ReturnsResult()
        {
            // Arrange
            var id = "/api/v1/appWebsite/productivity/app-id";
            var expectedResult = new ProductivityAppDeleteResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.DeleteWithTokenAsync<ProductivityAppDeleteResult>(
                    It.IsAny<string>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.DeleteProductivityApp(id);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region User Preferences Tests

        [Fact]
        public async Task GetEnableBeepSoundSetting_ReturnsResult()
        {
            // Arrange
            var url = "api/v1/userPreferences/getUserPreferenceByKey/TimeTracker.BlurScreenshots";
            var expectedResult = new EnableBeepSoundResult { data = true };

            _mockHttpProvider
                .Setup(x => x.GetWithTokenAsync<EnableBeepSoundResult>(
                    It.IsAny<string>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetEnableBeepSoundSetting(url);

            // Assert
            Assert.True(result.data);
        }

        [Fact]
        public async Task SetUserPreferences_ReturnsResult()
        {
            // Arrange
            var url = "api/v1/userPreferences/create";
            var request = new CreateUserPreferenceRequest
            {
                userId = "user-id",
                preferenceKey = "testKey",
                preferenceValue = "testValue"
            };
            var expectedResult = new BaseResponse { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<BaseResponse, CreateUserPreferenceRequest>(
                    It.IsAny<string>(), request, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.SetUserPreferences(url, request);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region Live Screen Tests

        [Fact]
        public async Task SendLiveScreenData_CallsHttpProvider()
        {
            // Arrange
            var request = new LiveImageRequest { fileString = "base64-image" };

            _mockHttpProvider
                .Setup(x => x.PostAsyncWithVoid(It.IsAny<string>(), request, "test-token"))
                .Returns(Task.CompletedTask);

            // Act
            await _restService.sendLiveScreenData(request);

            // Assert
            _mockHttpProvider.Verify(
                x => x.PostAsyncWithVoid(
                    It.Is<string>(s => s.Contains("/api/v1/liveTracking/save")),
                    request,
                    "test-token"),
                Times.Once);
        }

        [Fact]
        public async Task CheckLiveScreen_ReturnsResponse()
        {
            // Arrange
            var user = new TaskUser { user = "user-id" };
            var expectedResult = new CheckLiveScreenResponse { Success = true };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<CheckLiveScreenResponse, TaskUser>(
                    It.IsAny<string>(), user, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.checkLiveScreen(user);

            // Assert
            Assert.True(result.Success);
        }

        #endregion

        #region UpdateOnlineStatus Tests

        [Fact]
        public async Task UpdateOnlineStatus_ReturnsResult()
        {
            // Arrange
            var expectedResult = new UpdateOnlineStatusResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<UpdateOnlineStatusResult, object>(
                    It.IsAny<string>(), It.IsAny<object>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.UpdateOnlineStatus(
                "user-id", "machine-id", true, "project", "task");

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region Application Log Tests

        [Fact]
        public async Task AddUsedApplicationLog_ReturnsResult()
        {
            // Arrange
            var log = new ApplicationLog { appWebsite = "TestApp" };
            var expectedResult = new ApplicationLogResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.PostWithTokenAsync<ApplicationLogResult, ApplicationLog>(
                    It.IsAny<string>(), log, "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.AddUsedApplicationLog(log);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion

        #region Error Logs Tests

        [Fact]
        public async Task GetErrorLogs_ReturnsResult()
        {
            // Arrange
            var userId = "user-id";
            var expectedResult = new GetErrorLogResult { status = "success" };

            _mockHttpProvider
                .Setup(x => x.GetWithTokenAsync<GetErrorLogResult>(
                    It.IsAny<string>(), "test-token"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _restService.GetErrorLogs(userId);

            // Assert
            Assert.Equal("success", result.status);
        }

        #endregion
    }
}
