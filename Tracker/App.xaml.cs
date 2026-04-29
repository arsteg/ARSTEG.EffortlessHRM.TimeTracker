using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Services.Interfaces;
using TimeTracker.Trace;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// https://www.youtube.com/watch?v=nwsEi0JZM3k
    /// </summary>
    public partial class App : Application
    {
        private IRestService _restService;

        public App()
        {
            // Configure dependency injection
            var services = new ServiceCollection();
            services.AddApplicationServices();
            var serviceProvider = services.BuildServiceProvider();

            // Initialize the service locator for transitional support
            AppServiceProvider.Initialize(serviceProvider);

            // Get the REST service from DI
            _restService = serviceProvider.GetRequiredService<IRestService>();
        }

        protected override void OnStartup(StartupEventArgs e)
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

            // Register exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            // Show the main window
            base.OnStartup(e);
            // Set the ShutdownMode to OnExplicitShutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                try
                {
                    // Use Task.Run with timeout since this runs on system event thread
                    var updateTask = Task.Run(async () => await UpdateOnlineStatusAsync(false));
                    updateTask.Wait(TimeSpan.FromSeconds(3)); // Short timeout for sleep event
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error("Failed to update status on sleep", ex);
                }
            }
        }

        private void HandleException(Exception ex, string context)
        {
            LogManager.Logger.Error($"{context}: {ex.Message}", ex);
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        private async Task UpdateOnlineStatusAsync(bool isOnline)
        {
            try
            {
                var userId = GlobalSetting.Instance.LoginResult?.data?.user?.id;
                var machineId = GlobalSetting.Instance.MachineId;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(machineId))
                {
                    LogManager.Logger.Warn("User ID or Machine ID is missing for status update.");
                    return;
                }

                var result = await _restService.UpdateOnlineStatus(
                    userId,
                    machineId,
                    isOnline,
                    null,
                    null
                );
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error("Failed to update online status", ex);
                throw; // Re-throw to handle in caller if needed
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Unsubscribe from system events to prevent memory leak
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;

                // Dispose TimeTracker ViewModel to clean up timers and event handlers
                if (GlobalSetting.Instance.TimeTracker?.DataContext is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                }

                // Update online status - use timeout to prevent hanging on exit
                var updateTask = Task.Run(async () => await UpdateOnlineStatusAsync(false));
                updateTask.Wait(TimeSpan.FromSeconds(5)); // Timeout after 5 seconds
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error("Failed to update online status on exit", ex);
            }
            finally
            {
                MutexHandler.ReleaseMutex();
                base.OnExit(e);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    LogManager.Logger.Error("Unhandled exception", exception);
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
        }

        private void OnDispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e
        )
        {
            HandleException(e.Exception, "UI Thread Exception");
            e.Handled = true; // Prevent app crash (optional, based on your needs)
        }
    }

    public static class MutexHandler
    {
        private static Mutex mutex;

        public static bool CreateMutex()
        {
            mutex = new Mutex(true, "TimeTrackerMutex", out bool isNewInstance);
            return isNewInstance;
        }

        public static void ReleaseMutex()
        {
            mutex?.Close();
        }
    }
}
