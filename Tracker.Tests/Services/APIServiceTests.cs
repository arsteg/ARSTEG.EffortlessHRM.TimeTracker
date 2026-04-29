using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Services.Interfaces;
using Xunit;

namespace Tracker.Tests.Services
{
    public class APIServiceTests
    {
        private readonly Mock<IRestService> _mockRestService;
        private readonly APIService _apiService;

        public APIServiceTests()
        {
            _mockRestService = new Mock<IRestService>();
            _apiService = new APIService(_mockRestService.Object);
            SetupGlobalSetting();
        }

        private void SetupGlobalSetting()
        {
            GlobalSetting.Instance.LoginResult = new LoginResult
            {
                token = "test-token",
                data = new LoginresultData
                {
                    user = new Login { id = "test-user-id", email = "test@example.com" }
                }
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRestService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new APIService(null!));
        }

        [Fact]
        public void Constructor_WithValidRestService_CreatesInstance()
        {
            var service = new APIService(_mockRestService.Object);
            Assert.NotNull(service);
        }

        #endregion

        #region GetEnableBeepSoundSetting Tests

        [Fact]
        public async Task GetEnableBeepSoundSetting_ReturnsTrue_WhenDataIsTrue()
        {
            _mockRestService
                .Setup(x => x.GetEnableBeepSoundSetting(It.IsAny<string>()))
                .ReturnsAsync(new EnableBeepSoundResult { data = true });

            var result = await _apiService.GetEnableBeepSoundSetting();

            Assert.True(result);
        }

        [Fact]
        public async Task GetEnableBeepSoundSetting_ReturnsFalse_WhenDataIsFalse()
        {
            _mockRestService
                .Setup(x => x.GetEnableBeepSoundSetting(It.IsAny<string>()))
                .ReturnsAsync(new EnableBeepSoundResult { data = false });

            var result = await _apiService.GetEnableBeepSoundSetting();

            Assert.False(result);
        }

        [Fact]
        public async Task GetEnableBeepSoundSetting_ReturnsFalse_WhenResultIsNull()
        {
            _mockRestService
                .Setup(x => x.GetEnableBeepSoundSetting(It.IsAny<string>()))
                .ReturnsAsync((EnableBeepSoundResult)null!);

            var result = await _apiService.GetEnableBeepSoundSetting();

            Assert.False(result);
        }

        [Fact]
        public async Task GetEnableBeepSoundSetting_CallsRestService_WithCorrectEndpoint()
        {
            _mockRestService
                .Setup(x => x.GetEnableBeepSoundSetting(It.IsAny<string>()))
                .ReturnsAsync(new EnableBeepSoundResult { data = true });

            await _apiService.GetEnableBeepSoundSetting();

            _mockRestService.Verify(
                x => x.GetEnableBeepSoundSetting(
                    "api/v1/userPreferences/getUserPreferenceByKey/TimeTracker.BlurScreenshots"),
                Times.Once);
        }

        #endregion

        #region GetUserPreferencesSetting Tests

        [Fact]
        public async Task GetUserPreferencesSetting_ReturnsResult()
        {
            var expectedResult = new UserPreferenceResult
            {
                status = "success",
                isBeepSoundEnabled = true,
                isScreenshotNotificationEnabled = false,
                isBlurScreenshot = true
            };

            _mockRestService
                .Setup(x => x.GetUserPreferencesSetting(It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            var result = await _apiService.GetUserPreferencesSetting();

            Assert.Equal("success", result.status);
            Assert.True(result.isBeepSoundEnabled);
        }

        [Fact]
        public async Task GetUserPreferencesSetting_CallsRestService_WithCorrectUserId()
        {
            _mockRestService
                .Setup(x => x.GetUserPreferencesSetting(It.IsAny<string>()))
                .ReturnsAsync(new UserPreferenceResult());

            await _apiService.GetUserPreferencesSetting();

            _mockRestService.Verify(
                x => x.GetUserPreferencesSetting(
                    It.Is<string>(s => s.Contains("test-user-id"))),
                Times.Once);
        }

        #endregion

        #region SetUserPreferences Tests

        [Fact]
        public async Task SetUserPreferences_CallsRestService_WithCorrectData()
        {
            var request = new CreateUserPreferenceRequest
            {
                userId = "test-user",
                preferenceKey = "testKey",
                preferenceValue = "testValue"
            };

            _mockRestService
                .Setup(x => x.SetUserPreferences(It.IsAny<string>(), It.IsAny<CreateUserPreferenceRequest>()))
                .ReturnsAsync(new BaseResponse { status = "success" });

            var result = await _apiService.SetUserPreferences(request);

            _mockRestService.Verify(
                x => x.SetUserPreferences("api/v1/userPreferences/create", request),
                Times.Once);
            Assert.Equal("success", result.status);
        }

        [Fact]
        public async Task SetUserPreferences_ReturnsSuccessResponse()
        {
            var request = new CreateUserPreferenceRequest
            {
                userId = "user-id",
                preferenceKey = "key",
                preferenceValue = "value"
            };

            _mockRestService
                .Setup(x => x.SetUserPreferences(It.IsAny<string>(), request))
                .ReturnsAsync(new BaseResponse { status = "success", message = "Preference saved" });

            var result = await _apiService.SetUserPreferences(request);

            Assert.Equal("success", result.status);
            Assert.Equal("Preference saved", result.message);
        }

        #endregion

        #region GetUserPreferencesByKey Tests

        [Fact]
        public async Task GetUserPreferencesByKey_ReturnsProject_WhenFound()
        {
            var expectedProject = new Project
            {
                _id = "project-id",
                projectName = "Test Project"
            };

            _mockRestService
                .Setup(x => x.GetUserPreferencesSettingByKey(It.IsAny<string>()))
                .ReturnsAsync(expectedProject);

            var result = await _apiService.GetUserPreferencesByKey();

            Assert.NotNull(result);
            Assert.Equal("project-id", result._id);
            Assert.Equal("Test Project", result.projectName);
        }

        [Fact]
        public async Task GetUserPreferencesByKey_ReturnsNull_WhenNotFound()
        {
            _mockRestService
                .Setup(x => x.GetUserPreferencesSettingByKey(It.IsAny<string>()))
                .ReturnsAsync((Project)null!);

            var result = await _apiService.GetUserPreferencesByKey();

            Assert.Null(result);
        }

        #endregion
    }
}
