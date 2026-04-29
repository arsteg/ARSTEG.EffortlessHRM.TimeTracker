using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Services.Interfaces;
using TimeTracker.ViewModels;
using Xunit;

namespace Tracker.Tests.ViewModels
{
    public class TimeTrackerViewModelTests
    {
        private readonly Mock<IRestService> _mockRestService;
        private readonly Mock<IApiService> _mockApiService;

        public TimeTrackerViewModelTests()
        {
            _mockRestService = new Mock<IRestService>();
            _mockApiService = new Mock<IApiService>();
            SetupGlobalSetting();
            SetupDefaultMocks();
        }

        private void SetupGlobalSetting()
        {
            GlobalSetting.Instance.LoginResult = new LoginResult
            {
                token = "test-token",
                data = new LoginresultData
                {
                    user = new Login
                    {
                        id = "test-user-id",
                        email = "test@example.com",
                        firstName = "Test",
                        lastName = "User",
                        company = new Company { id = "company-id" }
                    }
                }
            };
        }

        private void SetupDefaultMocks()
        {
            _mockRestService
                .Setup(x => x.GetProjectListByUserId(It.IsAny<ProjectRequest>()))
                .ReturnsAsync(new GetProjectListAPIResult
                {
                    status = "success",
                    data = new ProjectList { projectList = new List<Project>() }
                });

            _mockRestService
                .Setup(x => x.GetCurrentWeekTotalTime(It.IsAny<CurrentWeekTotalTime>()))
                .ReturnsAsync(new GetTimeLogAPIResult
                {
                    status = "success",
                    data = new List<TimeLogBaseResponse>()
                });

            _mockRestService
                .Setup(x => x.GetTimeLogs(It.IsAny<TimeLog>()))
                .ReturnsAsync(new GetTimeLogAPIResult
                {
                    status = "success",
                    data = new List<TimeLogBaseResponse>()
                });

            _mockRestService
                .Setup(x => x.UpdateOnlineStatus(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new UpdateOnlineStatusResult { status = "success" });

            _mockRestService
                .Setup(x => x.checkLiveScreen(It.IsAny<TaskUser>()))
                .ReturnsAsync(new CheckLiveScreenResponse { Success = false });

            _mockApiService
                .Setup(x => x.GetUserPreferencesSetting())
                .ReturnsAsync(new UserPreferenceResult
                {
                    isBeepSoundEnabled = false,
                    isScreenshotNotificationEnabled = false,
                    isBlurScreenshot = false
                });

            _mockApiService
                .Setup(x => x.GetUserPreferencesByKey())
                .ReturnsAsync((Project)null!);
        }

        private TimeTrackerViewModel CreateViewModel()
        {
            return new TimeTrackerViewModel(_mockRestService.Object, _mockApiService.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRestService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TimeTrackerViewModel(null!, _mockApiService.Object));
        }

        [Fact]
        public void Constructor_WithNullApiService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TimeTrackerViewModel(_mockRestService.Object, null!));
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            var viewModel = CreateViewModel();

            Assert.NotNull(viewModel.CloseCommand);
            Assert.NotNull(viewModel.StartStopCommand);
            Assert.NotNull(viewModel.LogoutCommand);
            Assert.NotNull(viewModel.RefreshCommand);
            Assert.NotNull(viewModel.LogCommand);
            Assert.NotNull(viewModel.DeleteScreenshotCommand);
            Assert.NotNull(viewModel.SaveScreenshotCommand);
            Assert.NotNull(viewModel.OpenDashboardCommand);
            Assert.NotNull(viewModel.ProductivityApplicationCommand);
            Assert.NotNull(viewModel.TaskCompleteCommand);
            Assert.NotNull(viewModel.CreateNewTaskCommand);
            Assert.NotNull(viewModel.TaskOpenCommand);
            Assert.NotNull(viewModel.SwitchTrackerNoCommand);
            Assert.NotNull(viewModel.SwitchTrackerYesCommand);
        }

        [Fact]
        public void Constructor_SetsUserNameFromGlobalSetting()
        {
            var viewModel = CreateViewModel();
            Assert.Equal("test@example.com", viewModel.UserName);
        }

        [Fact]
        public void Constructor_SetsUserIdFromGlobalSetting()
        {
            var viewModel = CreateViewModel();
            Assert.Equal("test-user-id", viewModel.UserId);
        }

        [Fact]
        public void Constructor_InitializesStartStopButtonText()
        {
            var viewModel = CreateViewModel();
            Assert.Equal("Start", viewModel.StartStopButtonText);
        }

        #endregion

        #region Property Change Tests

        [Fact]
        public void Title_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.Title))
                    raised = true;
            };

            viewModel.Title = "New Title";

            Assert.True(raised);
            Assert.Equal("New Title", viewModel.Title);
        }

        [Fact]
        public void StartStopButtonText_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.StartStopButtonText))
                    raised = true;
            };

            viewModel.StartStopButtonText = "Stop";

            Assert.True(raised);
            Assert.Equal("Stop", viewModel.StartStopButtonText);
        }

        [Fact]
        public void CurrentInput_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.CurrentInput))
                    raised = true;
            };

            viewModel.CurrentInput = "test input";

            Assert.True(raised);
            Assert.Equal("test input", viewModel.CurrentInput);
        }

        [Fact]
        public void CurrentSessionTimeTracked_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.CurrentSessionTimeTracked))
                    raised = true;
            };

            viewModel.CurrentSessionTimeTracked = "01:30:00";

            Assert.True(raised);
            Assert.Equal("01:30:00", viewModel.CurrentSessionTimeTracked);
        }

        [Fact]
        public void CanSendReport_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.CanSendReport))
                    raised = true;
            };

            viewModel.CanSendReport = false;

            Assert.True(raised);
            Assert.False(viewModel.CanSendReport);
        }

        [Fact]
        public void ProgressWidthStart_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.ProgressWidthStart))
                    raised = true;
            };

            viewModel.ProgressWidthStart = 50;

            Assert.True(raised);
            Assert.Equal(50, viewModel.ProgressWidthStart);
        }

        [Fact]
        public void CanShowScreenshot_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.CanShowScreenshot))
                    raised = true;
            };

            viewModel.CanShowScreenshot = true;

            Assert.True(raised);
            Assert.True(viewModel.CanShowScreenshot);
        }

        [Fact]
        public void UserFullName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "UserFullName")
                    raised = true;
            };

            viewModel.UserFullName = "John Doe";

            Assert.True(raised);
            Assert.Equal("John Doe", viewModel.UserFullName);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void Title_DefaultValue_IsEmptyString()
        {
            var viewModel = CreateViewModel();
            Assert.Equal("", viewModel.Title);
        }

        [Fact]
        public void StartStopButtonText_DefaultValue_IsStart()
        {
            var viewModel = CreateViewModel();
            Assert.Equal("Start", viewModel.StartStopButtonText);
        }

        [Fact]
        public void CurrentInput_DefaultValue_IsEmptyString()
        {
            var viewModel = CreateViewModel();
            Assert.Equal(string.Empty, viewModel.CurrentInput);
        }

        [Fact]
        public void CanSendReport_DefaultValue_IsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.CanSendReport);
        }

        [Fact]
        public void ProgressWidthStart_DefaultValue_IsZero()
        {
            var viewModel = CreateViewModel();
            Assert.Equal(0, viewModel.ProgressWidthStart);
        }

        [Fact]
        public void CanShowScreenshot_DefaultValue_IsFalse()
        {
            var viewModel = CreateViewModel();
            Assert.False(viewModel.CanShowScreenshot);
        }

        #endregion

        #region Command CanExecute Tests

        [Fact]
        public void StartStopCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.StartStopCommand.CanExecute(null));
        }

        [Fact]
        public void LogoutCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.LogoutCommand.CanExecute(null));
        }

        [Fact]
        public void RefreshCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.RefreshCommand.CanExecute(null));
        }

        [Fact]
        public void LogCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.LogCommand.CanExecute(null));
        }

        [Fact]
        public void DeleteScreenshotCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.DeleteScreenshotCommand.CanExecute(null));
        }

        [Fact]
        public void SaveScreenshotCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = CreateViewModel();
            Assert.True(viewModel.SaveScreenshotCommand.CanExecute(null));
        }

        #endregion

        #region Property Getter/Setter Tests

        [Fact]
        public void VideoImage_GetterSetter_WorkCorrectly()
        {
            var viewModel = CreateViewModel();
            viewModel.VideoImage = "/path/to/video.mp4";
            Assert.Equal("/path/to/video.mp4", viewModel.VideoImage);
        }

        [Fact]
        public void DeleteImagePath_GetterSetter_WorkCorrectly()
        {
            var viewModel = CreateViewModel();
            viewModel.DeleteImagePath = "/path/to/delete.png";
            Assert.Equal("/path/to/delete.png", viewModel.DeleteImagePath);
        }

        [Fact]
        public void Taskname_GetterSetter_WorkCorrectly()
        {
            var viewModel = CreateViewModel();
            viewModel.Taskname = "Test Task";
            Assert.Equal("Test Task", viewModel.Taskname);
        }

        [Fact]
        public void Taskname_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeTrackerViewModel.Taskname))
                    raised = true;
            };

            viewModel.Taskname = "New Task";

            Assert.True(raised);
        }

        #endregion
    }
}
