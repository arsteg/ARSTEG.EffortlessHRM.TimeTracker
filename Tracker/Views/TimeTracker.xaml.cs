using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using TimeTracker.Models;
using TimeTracker.Trace;
using TimeTracker.ViewModels;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for TimeTracker.xaml
    /// </summary>
    public partial class TimeTracker : Window
    {
        public TimeTracker()
        {
            try
            {
                InitializeComponent();

                UpdatePopupPosition();
                System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
                ni.Icon = ConvertImageToIcon(
                    @$"{System.AppDomain.CurrentDomain.BaseDirectory}Media\Images\smallLogo.png"
                );
                ni.Text = "EffortlessHRM- Time Tracker";
                ni.Visible = true;
                ni.DoubleClick += delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

                // Register to listen for the message
                Messenger.Default.Register<NotificationMessage>(
                    this,
                    message =>
                    {
                        if (message.Notification == "RestoreWindowMethod")
                        {
                            RestoreWindow();
                        }
                    }
                );
                LoadUserSettings();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
        }

        private System.Drawing.Icon ConvertImageToIcon(string imagePath)
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                IntPtr hIcon = bitmap.GetHicon();
                return System.Drawing.Icon.FromHandle(hIcon);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
            UpdatePopupPosition();
        }

        // Minimize to system tray when application is closed.
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            e.Cancel = true;

            this.Hide();

            base.OnClosing(e);
        }

        private void UpdatePopupPosition()
        {
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            var popupWidth = popupControl.ActualWidth;
            var popupHeight = popupControl.ActualHeight;

            popupControl.PlacementRectangle = new Rect(
                screenWidth + 140,
                screenHeight - popupHeight,
                popupWidth,
                popupHeight
            );
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void btnActionMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void BtnActionMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnActionClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnActionClose_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
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
                LogManager.SetLoggingEnabled(logLevel, true);
            }
        }

        private void LoggingToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string logLevel)
            {
                LogManager.SetLoggingEnabled(logLevel, false);
            }
        }

        private void LoadUserSettings()
        {
            LogManager.SetLoggingEnabled("Info", Properties.Settings.Default.EnableInfoLogging);
            LogManager.SetLoggingEnabled("Warn", Properties.Settings.Default.EnableWarnLogging);
            LogManager.SetLoggingEnabled("Error", Properties.Settings.Default.EnableErrorLogging);

            // Update UI
            InfoCheckBox.IsChecked = Properties.Settings.Default.EnableInfoLogging;
            WarnCheckBox.IsChecked = Properties.Settings.Default.EnableWarnLogging;
            ErrorCheckBox.IsChecked = Properties.Settings.Default.EnableErrorLogging;
        }
    }
}
