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
    public TimeTrackerView()
    {
        InitializeComponent();
        this.Loaded += TimeTrackerView_Loaded;
    }

    private void TimeTrackerView_Loaded(object? sender, RoutedEventArgs e)
    {
        DataContext = new TimeTrackerViewModel(new ScreenshotService());

        Closing += async (s, e) =>
        {
            e.Cancel = true;
            Hide();
            await ((TimeTrackerViewModel)DataContext).CloseCommand.ExecuteAsync(null);
        };
    }

    private void DragBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void BtnActionMinimize_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void btnActionClose_Click(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

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
            Settings.Default.SetLoggingEnabled(logLevel, true);
        }
    }

    private void LoggingToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
        {
            Settings.Default.SetLoggingEnabled(logLevel, false);
        }
    }

    private void LoadUserSettings()
    {
        // Existing settings logic unchanged
    }
}

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
