using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using TimeTrackerX.Services;
using TimeTrackerX.ViewModels;

namespace TimeTrackerX.Views
{
    public partial class TimeTrackerView : Window
    {
        //private readonly SystemTrayIcon _trayIcon;
        //private readonly INotificationService _notificationService;

        public TimeTrackerView()
        {
            InitializeComponent();
            DataContext = new TimeTrackerViewModel(
            //App.Current._serviceProvider.GetService<IScreenshotService>(),
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

        //private SystemTrayIcon SetupSystemTray()
        //{
        //    var trayIcon = new SystemTrayIcon
        //    {
        //        Icon = new WindowIcon("Assets/smallLogo.png"),
        //        ToolTipText = "EffortlessHRM - Time Tracker"
        //    };
        //    trayIcon.Clicked += (s, e) =>
        //    {
        //        Show();
        //        WindowState = WindowState.Normal;
        //        Activate();
        //        Focus();
        //    };
        //    SystemTray.SetSystemTrayIcon(this, trayIcon);
        //    return trayIcon;
        //}

        private void BtnActionMinimize_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
        }

        private void BtnActionClose_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Hide();
            }
        }

        private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void LoggingToggle_Checked(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
            {
                Settings.Default.SetLoggingEnabled(logLevel, true);
                //_notificationService.Show(
                //    $"Logging {logLevel} Enabled",
                //    $"Logging for {logLevel} has been enabled."
                //);
            }
        }

        private void LoggingToggle_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
            {
                Settings.Default.SetLoggingEnabled(logLevel, false);
                //_notificationService.Show(
                //    $"Logging {logLevel} Disabled",
                //    $"Logging for {logLevel} has been disabled."
                //);
            }
        }

        private void LoadUserSettings()
        {
            //var settings = Settings.Default;
            //var vm = (TimeTrackerViewModel)DataContext;
            //vm.EnableInfoLogging = settings.EnableInfoLogging;
            //vm.EnableWarnLogging = settings.EnableWarnLogging;
            //vm.EnableErrorLogging = settings.EnableErrorLogging;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == WindowStateProperty && WindowState == WindowState.Minimized)
            {
                Hide();
            }
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
}
