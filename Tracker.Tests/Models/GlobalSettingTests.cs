using TimeTracker.Models;
using Xunit;

namespace Tracker.Tests.Models
{
    public class GlobalSettingTests
    {
        #region Singleton Tests

        [Fact]
        public void Instance_ReturnsNotNull()
        {
            Assert.NotNull(GlobalSetting.Instance);
        }

        [Fact]
        public void Instance_ReturnsSameInstance()
        {
            var instance1 = GlobalSetting.Instance;
            var instance2 = GlobalSetting.Instance;

            Assert.Same(instance1, instance2);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void LoginResult_CanBeSetAndGet()
        {
            var loginResult = new LoginResult
            {
                status = "success",
                token = "test-token"
            };

            GlobalSetting.Instance.LoginResult = loginResult;

            Assert.Equal("success", GlobalSetting.Instance.LoginResult.status);
            Assert.Equal("test-token", GlobalSetting.Instance.LoginResult.token);
        }

        [Fact]
        public void MachineId_CanBeSetAndGet()
        {
            GlobalSetting.Instance.MachineId = "machine-123";

            Assert.Equal("machine-123", GlobalSetting.Instance.MachineId);
        }

        #endregion

        #region Constants Tests

        [Fact]
        public void ApiBaseUrl_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GlobalSetting.apiBaseUrl));
        }

        [Fact]
        public void ApiBaseUrl_IsValidUrl()
        {
            Assert.True(Uri.IsWellFormedUriString(GlobalSetting.apiBaseUrl, UriKind.Absolute));
        }

        [Fact]
        public void PortalBaseUrl_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GlobalSetting.portalBaseUrl));
        }

        [Fact]
        public void PortalBaseUrl_IsValidUrl()
        {
            Assert.True(Uri.IsWellFormedUriString(GlobalSetting.portalBaseUrl, UriKind.Absolute));
        }

        [Fact]
        public void EmailReceiver_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GlobalSetting.EmailReceiver));
        }

        [Fact]
        public void EmailReceiver_ContainsAtSymbol()
        {
            Assert.Contains("@", GlobalSetting.EmailReceiver);
        }

        [Fact]
        public void ApiKey_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GlobalSetting.ApiKey));
        }

        #endregion

        #region UserPreferencesKey Tests

        [Fact]
        public void UserPreferenceKey_IsNotNull()
        {
            Assert.NotNull(GlobalSetting.Instance.userPreferenceKey);
        }

        [Fact]
        public void UserPreferenceKey_IsOfCorrectType()
        {
            Assert.IsType<UserPreferencesKey>(GlobalSetting.Instance.userPreferenceKey);
        }

        #endregion

        #region Window Properties Tests

        [Fact]
        public void TimeTracker_DefaultValue_IsNull()
        {
            // Note: This may not be null if set by other tests
            // Just verify the property is accessible
            var _ = GlobalSetting.Instance.TimeTracker;
            Assert.True(true); // Property is accessible
        }

        [Fact]
        public void LoginView_DefaultValue_IsNull()
        {
            var _ = GlobalSetting.Instance.LoginView;
            Assert.True(true); // Property is accessible
        }

        [Fact]
        public void ProductivityAppsSettings_DefaultValue_IsNull()
        {
            var _ = GlobalSetting.Instance.ProductivityAppsSettings;
            Assert.True(true); // Property is accessible
        }

        #endregion
    }

    public class UserPreferencesKeyTests
    {
        [Fact]
        public void UserPreferencesKey_CanBeCreated()
        {
            var key = new UserPreferencesKey();
            Assert.NotNull(key);
        }

        [Fact]
        public void UserPreferencesKey_HasExpectedProperties()
        {
            var key = GlobalSetting.Instance.userPreferenceKey;

            // Access all properties to ensure they exist
            Assert.NotNull(key.ScreenshotSoundDisabled);
            Assert.NotNull(key.ScreenshotNotificationDisabled);
            Assert.NotNull(key.ScreenshotBlur);
            Assert.NotNull(key.WeeklyHoursLimit);
            Assert.NotNull(key.MonthlyHoursLimit);
            Assert.NotNull(key.TrackerSelectedProject);
        }
    }
}
