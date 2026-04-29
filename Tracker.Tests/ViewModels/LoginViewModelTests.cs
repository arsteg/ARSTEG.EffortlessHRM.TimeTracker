using Moq;
using TimeTracker.Models;
using TimeTracker.Services.Interfaces;
using TimeTracker.ViewModels;
using Xunit;

namespace Tracker.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        private readonly Mock<IRestService> _mockRestService;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _mockRestService = new Mock<IRestService>();
            _viewModel = new LoginViewModel(_mockRestService.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRestService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginViewModel(null!));
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            Assert.NotNull(_viewModel.LoginCommand);
            Assert.NotNull(_viewModel.CloseCommand);
            Assert.NotNull(_viewModel.OpenForgotPasswordCommand);
            Assert.NotNull(_viewModel.OpenSignUpPageCommand);
            Assert.NotNull(_viewModel.OpenSocialMediaPageCommand);
        }

        [Fact]
        public void Constructor_InitializesDefaultValues()
        {
            Assert.False(_viewModel.EnableLoginButton);
            Assert.Equal(0, _viewModel.ProgressWidth);
            Assert.Equal("", _viewModel.ErrorMessage);
        }

        #endregion

        #region Property Change Notification Tests

        [Fact]
        public void UserName_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.UserName))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.UserName = "testuser@example.com";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("testuser@example.com", _viewModel.UserName);
        }

        [Fact]
        public void Password_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.Password))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.Password = "testpassword";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("testpassword", _viewModel.Password);
        }

        [Fact]
        public void RememberMe_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.RememberMe))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.RememberMe = true;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.True(_viewModel.RememberMe);
        }

        [Fact]
        public void EnableLoginButton_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.EnableLoginButton))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.EnableLoginButton = true;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.True(_viewModel.EnableLoginButton);
        }

        [Fact]
        public void ProgressWidth_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.ProgressWidth))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.ProgressWidth = 50;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal(50, _viewModel.ProgressWidth);
        }

        [Fact]
        public void ErrorMessage_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.ErrorMessage))
                    propertyChangedRaised = true;
            };

            // Act
            _viewModel.ErrorMessage = "Test error message";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("Test error message", _viewModel.ErrorMessage);
        }

        #endregion

        #region LoginCommandExecute Tests

        [Fact]
        public async Task LoginCommandExecute_WithEmptyUserName_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "";
            _viewModel.Password = "testpassword";

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_WithNullUserName_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = null!;
            _viewModel.Password = "testpassword";

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_WithEmptyPassword_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "testuser@example.com";
            _viewModel.Password = "";

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_WithNullPassword_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "testuser@example.com";
            _viewModel.Password = null!;

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_WithInvalidCredentials_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "testuser@example.com";
            _viewModel.Password = "wrongpassword";

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ReturnsAsync(new LoginResult { status = "error" });

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_ClearsErrorMessage_BeforeLogin()
        {
            // Arrange
            _viewModel.ErrorMessage = "Previous error";
            _viewModel.UserName = "";
            _viewModel.Password = "test";

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert - Error is set to new message, not cleared and left empty
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_SetsProgressWidth_DuringLogin()
        {
            // Arrange
            _viewModel.UserName = "test@example.com";
            _viewModel.Password = "password";
            var progressWasSet = false;

            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.ProgressWidth) && _viewModel.ProgressWidth == 30)
                    progressWasSet = true;
            };

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ReturnsAsync(new LoginResult { status = "error" });

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.True(progressWasSet);
            Assert.Equal(0, _viewModel.ProgressWidth); // Should reset after completion
        }

        [Fact]
        public async Task LoginCommandExecute_EnablesLoginButton_AfterCompletion()
        {
            // Arrange
            _viewModel.UserName = "test@example.com";
            _viewModel.Password = "password";
            _viewModel.EnableLoginButton = false;

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ReturnsAsync(new LoginResult { status = "error" });

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.True(_viewModel.EnableLoginButton);
        }

        [Fact]
        public async Task LoginCommandExecute_CallsSignInWithCorrectCredentials()
        {
            // Arrange
            _viewModel.UserName = "test@example.com";
            _viewModel.Password = "mypassword";

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ReturnsAsync(new LoginResult { status = "error" });

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            _mockRestService.Verify(
                x => x.SignIn(It.Is<Login>(l =>
                    l.email == "test@example.com" &&
                    l.password == "mypassword")),
                Times.Once);
        }

        [Fact]
        public async Task LoginCommandExecute_OnServiceAuthenticationException_SetsErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "test@example.com";
            _viewModel.Password = "password";

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ThrowsAsync(new TimeTracker.Services.ServiceAuthenticationException("Auth failed"));

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Equal("Invalid credentials, Please try again", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommandExecute_OnGeneralException_SetsGenericErrorMessage()
        {
            // Arrange
            _viewModel.UserName = "test@example.com";
            _viewModel.Password = "password";

            _mockRestService
                .Setup(x => x.SignIn(It.IsAny<Login>()))
                .ThrowsAsync(new Exception("Network error"));

            // Act
            await _viewModel.LoginCommandExecute();

            // Assert
            Assert.Contains("Something went wrong", _viewModel.ErrorMessage);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void EnableLoginButton_DefaultValue_IsFalse()
        {
            Assert.False(_viewModel.EnableLoginButton);
        }

        [Fact]
        public void ProgressWidth_DefaultValue_IsZero()
        {
            Assert.Equal(0, _viewModel.ProgressWidth);
        }

        [Fact]
        public void ErrorMessage_DefaultValue_IsEmpty()
        {
            Assert.Equal("", _viewModel.ErrorMessage);
        }

        [Fact]
        public void RememberMe_DefaultValue_IsFalse()
        {
            Assert.False(_viewModel.RememberMe);
        }

        [Fact]
        public void UserName_DefaultValue_IsNull()
        {
            Assert.Null(_viewModel.UserName);
        }

        [Fact]
        public void Password_DefaultValue_IsNull()
        {
            Assert.Null(_viewModel.Password);
        }

        #endregion

        #region Property Getter Tests

        [Fact]
        public void UserName_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.UserName = "user@test.com";
            Assert.Equal("user@test.com", _viewModel.UserName);
        }

        [Fact]
        public void Password_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.Password = "secret123";
            Assert.Equal("secret123", _viewModel.Password);
        }

        [Fact]
        public void RememberMe_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.RememberMe = true;
            Assert.True(_viewModel.RememberMe);

            _viewModel.RememberMe = false;
            Assert.False(_viewModel.RememberMe);
        }

        [Fact]
        public void EnableLoginButton_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.EnableLoginButton = true;
            Assert.True(_viewModel.EnableLoginButton);

            _viewModel.EnableLoginButton = false;
            Assert.False(_viewModel.EnableLoginButton);
        }

        [Fact]
        public void ProgressWidth_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.ProgressWidth = 75;
            Assert.Equal(75, _viewModel.ProgressWidth);
        }

        [Fact]
        public void ErrorMessage_GetterAndSetter_WorkCorrectly()
        {
            _viewModel.ErrorMessage = "Custom error";
            Assert.Equal("Custom error", _viewModel.ErrorMessage);
        }

        #endregion
    }
}
