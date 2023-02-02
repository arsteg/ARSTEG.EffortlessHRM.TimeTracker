using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for TimeTracker.xaml
    /// </summary>
    public partial class TimeTracker : Window
    {
        public TimeTracker()
        {
            InitializeComponent();
            UpdatePopupPosition();
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon(@$"{System.AppDomain.CurrentDomain.BaseDirectory}\Media\Images\logo.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            //SetTheme();

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
              popupHeight);
        }
    }
}
