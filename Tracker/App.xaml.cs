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
	/// https://www.youtube.com/watch?v=nwsEi0JZM3k
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup(e);

			// Create the main window			

			// Check if the application is already running
			bool isAlreadyRunning = !MutexHandler.CreateMutex();

			if (isAlreadyRunning)
			{
				// TODO: Handle the case when the application is already running
				MessageBox.Show("An instance is already running...");
				Shutdown();
				return;
			}

			// Show the main window
			base.OnStartup(e);
			// Set the ShutdownMode to OnExplicitShutdown
			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

		}

		protected override void OnExit( ExitEventArgs e )
		{
			// Release the mutex when the application exits
			MutexHandler.ReleaseMutex();
			base.OnExit(e);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            MessageBox.Show(exception.Message);
            // Handle the exception here (e.g., log the exception details, show a user-friendly error message)
        }
    }

	public static class MutexHandler
	{
		private static Mutex mutex;

		public static bool CreateMutex()
		{
			mutex = new Mutex(true, "YourAppMutex", out bool isNewInstance);
			return isNewInstance;
		}

		public static void ReleaseMutex()
		{
			mutex?.Close();
		}
	}
}
