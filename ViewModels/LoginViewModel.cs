using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Utilities;

namespace TimeTracker.ViewModels
{
    public class LoginViewModel:ViewModelBase
    {
        #region private members
        #endregion

        #region constructor
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(LoginCommandExecute);
            CloseCommand = new RelayCommand(CloseCommandExecute);
        }
        #endregion

        #region commands
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }

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

        #endregion

        #region public methods
        public  async void LoginCommandExecute() {
            try
            {
                var rest = new REST(new HttpProviders());

                var result = await rest.SignIn(new Models.Login() { email = UserName, password = Password });

                if (result.status == "success")
                {
                    GlobalSetting.Instance.LoginResult = result;
                    GlobalSetting.Instance.TimeTracker = new TimeTracker.Views.TimeTracker();
                    GlobalSetting.Instance.TimeTracker.Show();
                    Application.Current.MainWindow.Close();
                }
                else
                {
                    MessageBox.Show("Invalid credentials, Please try again");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Invalid credentials, Please try again");
            }
        }
        public void CloseCommandExecute()
        {
            Application.Current.Shutdown();
        }
        #endregion 
    }
}
