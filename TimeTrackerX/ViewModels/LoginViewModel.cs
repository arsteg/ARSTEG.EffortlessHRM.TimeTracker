using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using TimeTrackerX.Models;
using TimeTrackerX.Services;
using TimeTrackerX.ViewModels;
using NLog;

namespace TimeTrackerX.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region private members
        private IConfiguration configuration;
        private static Logger logger;
        #endregion

        #region constructor
        public LoginViewModel()
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("LoginViewModel constructor starts");

            this.configuration = configuration;
            LoginCommand = new RelayCommand(async () => await LoginCommandExecuteAsync());
            CloseCommand = new RelayCommand(CloseCommandExecute);
            OpenForgotPasswordCommand = new RelayCommand(OpenForgotPasswordCommandExecute);
            OpenSignUpPageCommand = new RelayCommand(OpenSignUpPageCommandExecute);
            OpenSocialMediaPageCommand = new RelayCommand<string>(
                OpenSocialMediaPageCommandCommandExecute
            );

            logger.Info("LoginViewModel constructor ends");
        }
        #endregion

        #region commands
        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand OpenForgotPasswordCommand { get; set; }
        public RelayCommand OpenSignUpPageCommand { get; set; }
        public RelayCommand<string> OpenSocialMediaPageCommand { get; set; }
        #endregion

        #region public properties
        private string username;
        public string UserName
        {
            get { return username; }
            set
            {
                logger.Debug($"UserName set to: {value}");
                username = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                logger.Debug("Password set");
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private bool rememberMe;
        public bool RememberMe
        {
            get { return rememberMe; }
            set
            {
                logger.Info("RememberMe setter starts");
                logger.Debug($"RememberMe set to: {value}");
                rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
                SaveUserCredentials(
                    rememberMe,
                    rememberMe ? UserName : "",
                    rememberMe ? Password : ""
                );
                logger.Info("RememberMe setter ends");
            }
        }

        private bool enableLoginButton = true;
        public bool EnableLoginButton
        {
            get { return enableLoginButton; }
            set
            {
                logger.Debug($"EnableLoginButton set to: {value}");
                enableLoginButton = value;
                OnPropertyChanged(nameof(EnableLoginButton));
            }
        }

        private int progressWidth = 0;
        public int ProgressWidth
        {
            get { return progressWidth; }
            set
            {
                logger.Debug($"ProgressWidth set to: {value}");
                progressWidth = value;
                OnPropertyChanged(nameof(ProgressWidth));
            }
        }

        private string errorMessage = "";
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                logger.Debug($"ErrorMessage set to: {value}");
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        #endregion

        #region public methods
        public async Task LoginCommandExecute()
        {
            logger.Info("LoginCommandExecute starts");

            EnableLoginButton = false;
            try
            {
                ErrorMessage = "";
                logger.Debug("Validating credentials");

                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Invalid credentials, Please try again";
                    logger.Warn("Empty username or password");
                    return;
                }

                ProgressWidth = 30;

                logger.Info("Calling REST.SignIn");
                var rest = new REST(new HttpProviders());

                var result = await rest.SignIn(
                    new Models.Login() { email = UserName, password = Password }
                );

                if (result.status == "success")
                {
                    logger.Info("SignIn succeeded");
                    GlobalSetting.Instance.LoginResult = result;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        logger.Info("Closing LoginView if exists");
                        if (GlobalSetting.Instance.LoginView != null)
                        {
                            GlobalSetting.Instance.LoginView.Close();
                            GlobalSetting.Instance.LoginView = null;
                        }

                        if (GlobalSetting.Instance.TimeTracker == null)
                        {
                            logger.Info("Creating TimeTracker instance");
                            GlobalSetting.Instance.TimeTracker =
                                new TimeTrackerX.Views.TimeTrackerView();
                        }

                        logger.Info("Showing TimeTracker");
                        GlobalSetting.Instance.TimeTracker.Show();
                    });

                    SaveUserCredentials(
                        rememberMe,
                        rememberMe ? UserName : "",
                        rememberMe ? Password : ""
                    );
                }
                else
                {
                    ErrorMessage = "Invalid credentials, Please try again";
                    logger.Warn("SignIn returned non-success status");
                }

                logger.Info("LoginCommandExecute ends");
            }
            catch (ServiceAuthenticationException ex)
            {
                ErrorMessage = "Invalid credentials, Please try again";
                logger.Error(ex, "ServiceAuthenticationException occurred");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Something went wrong, Please try again.\n {ex.InnerException?.Message}";
                logger.Error(ex, "Unexpected exception occurred during login");
            }
            finally
            {
                ProgressWidth = 0;
                EnableLoginButton = true;
            }
        }

        public void CloseCommandExecute()
        {
            logger.Info("CloseCommandExecute called");
            // Application.Current.Shutdown(); // Not used currently
        }

        public void OpenForgotPasswordCommandExecute()
        {
            logger.Info("OpenForgotPasswordCommandExecute called");
            try
            {
                string url = configuration.GetSection("ApplicationBaseUrl").Value + "#/forgotPassword";
                logger.Debug($"Opening Forgot Password URL: {url}");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open Forgot Password page");
            }
        }

        public void OpenSignUpPageCommandExecute()
        {
            logger.Info("OpenSignUpPageCommandExecute called");
            try
            {
                string url = configuration.GetSection("SignUpUrl").Value;
                logger.Debug($"Opening SignUp URL: {url}");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open Sign Up page");
            }
        }

        public void OpenSocialMediaPageCommandCommandExecute(string pageName)
        {
            logger.Info($"OpenSocialMediaPageCommandCommandExecute called with page: {pageName}");
            try
            {
                string url = configuration.GetSection(pageName).Value;
                logger.Debug($"Opening social media URL: {url}");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to open social media page: {pageName}");
            }
        }
        #endregion

        #region Private Methods
        private async Task LoginCommandExecuteAsync()
        {
            logger.Info("LoginCommandExecuteAsync called");
            await LoginCommandExecute();
        }

        private void SaveUserCredentials(bool rememberMe, string userName, string password)
        {
            logger.Info("SaveUserCredentials called");
            logger.Debug($"rememberMe: {rememberMe}, userName: {userName}, password: {(string.IsNullOrEmpty(password) ? "[EMPTY]" : "[SET]")}");

            try
            {
                if (rememberMe)
                {
                    Properties.Settings.Default.userName = userName;
                    Properties.Settings.Default.userPassword = password;
                    Properties.Settings.Default.Save();
                    logger.Info("User credentials saved");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to save user credentials");
            }
        }

        #endregion
    }
}
