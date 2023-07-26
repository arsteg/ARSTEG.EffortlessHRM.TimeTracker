using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TimeTracker.Models;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool aIsNewInstance = false;

            mutex = new Mutex(true, "TimeTracker", out aIsNewInstance);

            if (!aIsNewInstance)
            {
                MessageBox.Show("An instance is already running...");
                System.Windows.Application.Current.Shutdown();
            }

            base.OnStartup(e);
            // Set the ShutdownMode to OnExplicitShutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;           

        }
    }
}
