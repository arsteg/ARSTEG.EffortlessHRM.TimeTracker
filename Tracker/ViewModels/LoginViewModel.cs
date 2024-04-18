using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Extensions.Configuration;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Trace;
using TimeTracker.Utilities;

namespace TimeTracker.ViewModels
{
    public class LoginViewModel:ViewModelBase
    {
        #region private members
        private IConfiguration configuration;
        #endregion

        #region constructor
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(LoginCommandExecute);
            CloseCommand = new RelayCommand(CloseCommandExecute);
            OpenForgotPasswordCommand = new RelayCommand(OpenForgotPasswordCommandExecute);
            OpenSignUpPageCommand = new RelayCommand(OpenSignUpPageCommandExecute);            
			OpenSocialMediaPageCommand = new RelayCommand<string>(OpenSocialMediaPageCommandCommandExecute);            
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();        }
        #endregion

        #region commands
        public RelayCommand LoginCommand { get; set; }
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
            set { 
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
                rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
                if (rememberMe)
                {
                    Properties.Settings.Default.userName = UserName;
                    Properties.Settings.Default.userPassword = Password;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.userName = "";
                    Properties.Settings.Default.userPassword = "";
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool enableLoginButton= false;
        public bool EnableLoginButton
        {
            get { return enableLoginButton; }
            set
            {
                enableLoginButton = value;
                OnPropertyChanged(nameof(EnableLoginButton));               
            }
        }

        private int progressWidth =   0;
        public int ProgressWidth
        {
            get { return progressWidth;}
            set
            {
                progressWidth = value;
                OnPropertyChanged(nameof(ProgressWidth));               
            }
        }

        private string  errorMessage= ""  ;
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
        public  async void LoginCommandExecute() {
            try
            {
                if( string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {                    
                    return;
                }
                ProgressWidth = 30;
                ErrorMessage = "";               

				var rest = new REST(new HttpProviders());
                
                var result = await rest.SignIn(new Models.Login() { email = UserName, password = Password });

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
                        GlobalSetting.Instance.TimeTracker = new TimeTracker.Views.TimeTracker();
                    }
					LogManager.Logger.Info("showing the instance of TimeTracker");
					GlobalSetting.Instance.TimeTracker.Show();
                    Application.Current.MainWindow.Close();
					LogManager.Logger.Info("TimeTracker is loaded.");

					if (rememberMe)
                    {
                        Properties.Settings.Default.userName = UserName;
                        Properties.Settings.Default.userPassword = Password;                        
                    }
                    else {
                        Properties.Settings.Default.userName = "";
                        Properties.Settings.Default.userPassword = "";                        
                    }
                    Properties.Settings.Default.Save();
                }
                else
                {
                    ErrorMessage ="Invalid credentials, Please try again";
                }
            }
            catch(ServiceAuthenticationException ex)
            {
                ErrorMessage = "Invalid credentials, Please try again";
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Something went wrong, Please try again.\n {ex.InnerException?.Message}";
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
            Application.Current.Shutdown();
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
        public void OpenSocialMediaPageCommandCommandExecute( string pageName)
        {
			string  url = configuration.GetSection(pageName).Value;
			            
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }       
        
        #endregion
    }
}
