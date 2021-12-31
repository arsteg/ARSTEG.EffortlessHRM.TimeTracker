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
        private string username="mohdrafionline1@gmail.com";
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
