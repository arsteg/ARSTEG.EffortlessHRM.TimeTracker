using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using TimeTrackerX.Models;
using TimeTrackerX.Services;
using TimeTrackerX.Trace;
using TimeTrackerX.ViewModels;

namespace TimeTrackerX.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region private members
        private IConfiguration configuration;
        #endregion

        #region constructor
        public LoginViewModel()
        {
            this.configuration = configuration;
            LogManager.Logger.Info($"constructor starts");
            LoginCommand = new RelayCommand(async () => await LoginCommandExecuteAsync());
            CloseCommand = new RelayCommand(CloseCommandExecute);
            OpenForgotPasswordCommand = new RelayCommand(OpenForgotPasswordCommandExecute);
            OpenSignUpPageCommand = new RelayCommand(OpenSignUpPageCommandExecute);
            OpenSocialMediaPageCommand = new RelayCommand<string>(
                OpenSocialMediaPageCommandCommandExecute
            );
            //configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            LogManager.Logger.Info($"constructor ends");
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
                LogManager.Logger.Info($"remember me setter starts");
                rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
                SaveUserCredentials(
                    rememberMe,
                    rememberMe ? UserName : "",
                    rememberMe ? Password : ""
                );
                LogManager.Logger.Info($"remember me setter ends");
            }
        }

        private bool enableLoginButton = false;
        public bool EnableLoginButton
        {
            get { return enableLoginButton; }
            set
            {
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
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        #endregion

        #region public methods
        public async Task LoginCommandExecute()
        {
            try
            {
                ErrorMessage = "";
                LogManager.Logger.Info($"login command execution starts");
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Invalid credentials, Please try again";
                    return;
                }
                ProgressWidth = 30;
                ErrorMessage = "";

                var rest = new REST(new HttpProviders());

                var result = await rest.SignIn(
                    new Models.Login() { email = UserName, password = Password }
                );

                if (result.status == "success")
                {
                    LogManager.Logger.Info("SignIn is successful");
                    if (GlobalSetting.Instance.LoginView != null)
                    {
                        GlobalSetting.Instance.LoginView.Close();
                        GlobalSetting.Instance.LoginView = null;
                    }

                    GlobalSetting.Instance.LoginResult = result;
                    if (GlobalSetting.Instance.TimeTracker == null)
                    {
                        LogManager.Logger.Info("Creating the instance of TimeTracker");
                        GlobalSetting.Instance.TimeTracker =
                            new TimeTrackerX.Views.TimeTrackerView();
                    }
                    LogManager.Logger.Info("showing the instance of TimeTracker");
                    GlobalSetting.Instance.TimeTracker.Show();
                    //Application.Current.MainWindow?.Close();
                    LogManager.Logger.Info("TimeTracker is loaded.");

                    SaveUserCredentials(
                        rememberMe,
                        rememberMe ? UserName : "",
                        rememberMe ? Password : ""
                    );
                }
                else
                {
                    ErrorMessage = "Invalid credentials, Please try again";
                }
                LogManager.Logger.Info($"login command execution ends");
            }
            catch (ServiceAuthenticationException ex)
            {
                ErrorMessage = "Invalid credentials, Please try again";
                LogManager.Logger.Error(ex);
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Something went wrong, Please try again.\n {ex.InnerException?.Message}";
                LogManager.Logger.Error(ex);
            }
            finally
            {
                ProgressWidth = 0;
                EnableLoginButton = true;
            }
        }

        public void CloseCommandExecute()
        {
            //Application.Current.Shutdown();
        }

        public void OpenForgotPasswordCommandExecute()
        {
            string url = configuration.GetSection("ApplicationBaseUrl").Value + "#/forgotPassword";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        public void OpenSignUpPageCommandExecute()
        {
            string url = configuration.GetSection("SignUpUrl").Value;
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        public void OpenSocialMediaPageCommandCommandExecute(string pageName)
        {
            string url = configuration.GetSection(pageName).Value;

            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        #endregion

        #region Private Methods
        private async Task LoginCommandExecuteAsync()
        {
            await LoginCommandExecute();
        }

        private void SaveUserCredentials(bool rememberMe, string userName, string password)
        {
            if (rememberMe)
            {
                Properties.Settings.Default.userName = userName;
                Properties.Settings.Default.userPassword = password;
                //Properties.Settings.Default.Save();
            }
        }

        #endregion
    }
}
