using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Extensions.DependencyInjection;
using TimeTrackerX.Services;
using TimeTrackerX.Services.Implementation;
using TimeTrackerX.Services.Interfaces;
using TimeTrackerX.ViewModels;

namespace TimeTrackerX.Views;

public partial class TimeTrackerView : Window
{
    //private readonly SystemTrayIcon _trayIcon;
    //private readonly INotificationService _notificationService;

    public TimeTrackerView()
    {
        InitializeComponent();
        this.Loaded += TimeTrackerView_Loaded;
    }

    private void TimeTrackerView_Loaded(object? sender, RoutedEventArgs e)
    {
        DataContext = new TimeTrackerViewModel(
            new ScreenshotService()
        //App.Current._serviceProvider.GetService<IMouseEventService>(),
        //App.Current._serviceProvider.GetService<IKeyEventService>(),
        //App.Current._serviceProvider.GetService<REST>()
        );

        //_notificationService = App.Current._serviceProvider.GetService<INotificationService>();
        //_trayIcon = SetupSystemTray();
        LoadUserSettings();

        Closing += async (s, e) =>
        {
            e.Cancel = true;
            Hide();
            await ((TimeTrackerViewModel)DataContext).CloseCommand.ExecuteAsync(null);
        };
    }

    //private MsBox.Avalonia.Enums.Icon ConvertImageToIcon(string imagePath)
    //{
    //    using (Bitmap bitmap = new Bitmap(imagePath))
    //    {
    //        IntPtr hIcon = bitmap.GetHicon();
    //        return System.Drawing.Icon.FromHandle(hIcon);
    //    }
    //}

    //protected override void OnStateChanged(EventArgs e)
    //{
    //    if (WindowState == WindowState.Minimized)
    //    {
    //        this.Hide();
    //    }
    //    base.OnStateChanged(e);
    //    UpdatePopupPosition();
    //}

    // Minimize to system tray when application is closed.
    //protected override void OnClosing(CancelEventArgs e)
    //{
    //    // setting cancel to true will cancel the close request
    //    // so the application is not closed
    //    e.Cancel = true;

    //    this.Hide();

    //    base.OnClosing(e);
    //}

    private void UpdatePopupPosition()
    {
        //var screenWidth = SystemParameters.WorkArea.Width;
        //var screenHeight = SystemParameters.WorkArea.Height;
        //var popupWidth = popupControl.ActualWidth;
        //var popupHeight = popupControl.ActualHeight;

        //popupControl.PlacementRectangle = new Rect(
        //    screenWidth + 140,
        //    screenHeight - popupHeight,
        //    popupWidth,
        //    popupHeight
        //);
    }

    //private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    //{
    //    Left = Left + e.HorizontalChange;
    //    Top = Top + e.VerticalChange;
    //}

    private void btnActionMinimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    //private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    if (e.ChangedButton == MouseButton.Left)
    //    {
    //        //this.DragMove();
    //    }
    //}

    private void BtnActionMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void btnActionClose_Click(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    //private void btnActionClose_Click(object sender, MouseButtonEventArgs e)
    //{
    //    //Application.Current.Shutdown();
    //}

    public void RestoreWindow()
    {
        if (this.WindowState == WindowState.Minimized)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        this.Activate();
        this.Focus();
    }

    private void LoggingToggle_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
        {
            //LogManager.SetLoggingEnabled(logLevel, true);
        }
    }

    private void LoggingToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
        {
            //LogManager.SetLoggingEnabled(logLevel, false);
        }
    }

    private void LoadUserSettings()
    {
        //LogManager.SetLoggingEnabled("Info", Properties.Settings.Default.EnableInfoLogging);
        //LogManager.SetLoggingEnabled("Warn", Properties.Settings.Default.EnableWarnLogging);
        //LogManager.SetLoggingEnabled("Error", Properties.Settings.Default.EnableErrorLogging);

        //// Update UI
        //InfoCheckBox.IsChecked = Properties.Settings.Default.EnableInfoLogging;
        //WarnCheckBox.IsChecked = Properties.Settings.Default.EnableWarnLogging;
        //ErrorCheckBox.IsChecked = Properties.Settings.Default.EnableErrorLogging;
    }
}

// Placeholder settings
public static class Settings
{
    public static class Default
    {
        public static bool EnableInfoLogging { get; set; } = true;
        public static bool EnableWarnLogging { get; set; } = true;
        public static bool EnableErrorLogging { get; set; } = true;

        public static void SetLoggingEnabled(string logLevel, bool enabled)
        {
            switch (logLevel)
            {
                case "Info":
                    EnableInfoLogging = enabled;
                    break;
                case "Warn":
                    EnableWarnLogging = enabled;
                    break;
                case "Error":
                    EnableErrorLogging = enabled;
                    break;
            }
        }
    }
}
