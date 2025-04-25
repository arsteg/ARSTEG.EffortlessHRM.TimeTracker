using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Drawing.Charts;
using TimeTrackerX.Models;
using TimeTrackerX.ViewModels;

namespace TimeTrackerX;

public partial class LoginView : Window
{
    private bool automaticLogin = true;

    public LoginView()
    {
        InitializeComponent();
        this.Loaded += Login_Loaded;
        GlobalSetting.Instance.LoginView = this;
    }

    private async void Login_Loaded(object sender, RoutedEventArgs e)
    {
        DataContext = new LoginViewModel();

        if (Properties.Settings.Default.userName != string.Empty)
        {
            ((LoginViewModel)this.DataContext).UserName = Properties.Settings.Default.userName;
        }
        if (Properties.Settings.Default.userPassword != string.Empty)
        {
            this.txtPassword.Text = Properties.Settings.Default.userPassword;
            this.chkRememberMe.IsChecked = true;
        }
        if (this.automaticLogin)
        {
            ((LoginViewModel)this.DataContext).LoginCommandExecute();
        }

        try
        {
            //LogManager.Logger.Info("calling GitHubUpdateManager");

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
            //LogManager.Logger.Error(ex);
        }
    }

    private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text.Length > 0)
        {
            passwordHint.IsVisible = false;
        }
        else
        {
            passwordHint.IsVisible = true;
        }

        if (this.DataContext != null)
        {
            // ((LoginViewModel)this.DataContext).Password = ((PasswordBox)sender).Password;
        }
    }

    private void CheckBoxChanged(object sender, RoutedEventArgs e) { }

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
        //System.Windows.Application.Current.Shutdown();
    }

    //private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    //{
    //    Left = Left + e.HorizontalChange;
    //    Top = Top + e.VerticalChange;
    //}

    //private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    txtUsername.Focus();
    //}

    private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(txtUsername.Text) && txtUsername.Text.Length > 0)
        {
            textEmail.IsVisible = false;
        }
        else
        {
            textEmail.IsVisible = true;
        }
    }

    //private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    txtPassword.Focus();
    //}

    //private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    if (e.ChangedButton == MouseButton.Left)
    //    {
    //        this.DragMove();
    //    }
    //}

    //private void btnActionClose_Click(object sender, MouseButtonEventArgs e)
    //{
    //    //System.Windows.Application.Current.Shutdown();
    //}
}
