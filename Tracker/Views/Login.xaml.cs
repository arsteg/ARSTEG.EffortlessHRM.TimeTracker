using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using TimeTracker.ViewModels;
using System.Threading;
using System.Windows.Controls.Primitives;
//using Squirrel;
using TimeTracker.Trace;
using DocumentFormat.OpenXml.ExtendedProperties;



namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        //UpdateManager manager;

        private bool automaticLogin = true;
        public Login()
        {
            InitializeComponent();
            this.Loaded += Login_Loaded;
        }
        public Login(bool automaticLogin=true)
        {
            InitializeComponent();
            this.automaticLogin = automaticLogin;
            this.Loaded += Login_Loaded;
        }

        private async void Login_Loaded( object sender, RoutedEventArgs e )
        {
            if (Properties.Settings.Default.userName != string.Empty)
            {
                ((LoginViewModel)this.DataContext).UserName = Properties.Settings.Default.userName;
            }
            if (Properties.Settings.Default.userPassword != string.Empty)
            {
                this.txtPassword.Password = Properties.Settings.Default.userPassword;
                this.chkRememberMe.IsChecked = true;
            }
            if (this.automaticLogin)
            {
                ((LoginViewModel)this.DataContext).LoginCommandExecute();
            }
#if RELEASE
			try
			{
				LogManager.Logger.Info("calling GitHubUpdateManager");               
                

				//manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/arsteg/ARSTEG.EffortlessHRM.TimeTracker");
				//if (manager != null)
				//{
				//	LogManager.Logger.Info($"calling CheckForUpdate");
				//	var updateInfo = await manager.CheckForUpdate();
				//	if (updateInfo.ReleasesToApply.Count > 0)
				//	{
				//		LogManager.Logger.Info($"updating the application");						
				//		await manager.UpdateApp();
				//		MessageBox.Show("updated successfully");
				//	}
				//	currentVersion.Text = manager.CurrentlyInstalledVersion().ToString();
				//	LogManager.Logger.Info($"updating the currentVersion");
				//}
				//else
				//{
				//	LogManager.Logger.Info($"manager = {manager}");
				//}
			}
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
#endif

		}

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Password) && txtPassword.Password.Length > 0)
            {
                passwordHint.Visibility = Visibility.Collapsed;
            }
            else
            {
                passwordHint.Visibility = Visibility.Visible;
            }

            if (this.DataContext != null)
            { ((LoginViewModel)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }


        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnActionMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnActionSystemInformation_OnClick(object sender, RoutedEventArgs e)
        {
            //var systemInformationWindow = new SystemInformationWindow();
            //systemInformationWindow.Show();
        }

        private void btnActionClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUsername.Focus();
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtUsername.Text) && txtUsername.Text.Length > 0)
            {
                textEmail.Visibility = Visibility.Collapsed;
            }
            else
            {
                textEmail.Visibility = Visibility.Visible;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword.Focus();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }
        private void btnActionClose_Click( object sender, MouseButtonEventArgs e )
        {
			System.Windows.Application.Current.Shutdown();
        }
    }
}
