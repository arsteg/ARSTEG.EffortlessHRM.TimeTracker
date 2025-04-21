using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using TimeTrackerX.Models;
using TimeTrackerX.Services;
using TimeTrackerX.Services.Implementation;
using TimeTrackerX.Services.Interfaces;

namespace TimeTrackerX
{
    public partial class App : Application
    {
        private readonly REST _restService;
        public IServiceProvider _serviceProvider;

        public App()
        {
            _restService = new REST(new HttpProviders());
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Disable Avalonia's DataAnnotations validation
                var dataValidationPluginsToRemove = BindingPlugins
                    .DataValidators.OfType<DataAnnotationsValidationPlugin>()
                    .ToArray();
                foreach (var plugin in dataValidationPluginsToRemove)
                {
                    BindingPlugins.DataValidators.Remove(plugin);
                }

                // Check for single instance
                bool isAlreadyRunning = !MutexHandler.CreateMutex();
                if (isAlreadyRunning)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        //await MessageBoxManager.GetMessageBoxStandardWindow(
                        //        new MessageBoxStandardParams
                        //        {
                        //            ContentTitle = "Error",
                        //            ContentMessage = "An instance is already running...",
                        //            ButtonDefinitions = ButtonEnum.Ok,
                        //            Icon = Icon.Error,
                        //            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        //        }
                        //    )
                        //    .ShowAsync();
                        desktop.Shutdown();
                    });
                    return;
                }

                // Configure services
                var services = new ServiceCollection();
                services.AddSingleton(_restService);
                services.AddSingleton<IScreenshotService, ScreenshotService>();
                //                services.AddSingleton<IScreenshotService>(
                //#if MACOS
                //                    new Platforms.MacOS.ScreenshotService()
                //#elif WINDOWS
                //                    new Platforms.Windows.ScreenshotService()
                //#else
                //                //throw new PlatformNotSupportedException("Screenshot service not implemented.")
                //#endif
                //                );
                //                services.AddSingleton<IMouseEventService>(
                //#if MACOS
                //                    new Platforms.MacOS.MouseEventService()
                //#else
                //                //throw new PlatformNotSupportedException("Mouse event service not implemented.")
                //#endif
                //                );
                //                services.AddSingleton<IKeyEventService>(
                //#if MACOS
                //                    new Platforms.MacOS.KeyEventService()
                //#elif WINDOWS
                //                    new Platforms.Windows.KeyEventService()
                //#elif LINUX
                //                    new Platforms.Linux.KeyEventService()
                //#else
                //                //throw new PlatformNotSupportedException("Key event service not implemented.")
                //#endif
                //                );
                //                services.AddSingleton<IPowerModeService>(
                //#if MACOS
                //                    new Platforms.MacOS.PowerModeService()
                //#else
                //                    null
                //#endif
                //                );
                _serviceProvider = services.BuildServiceProvider();

                // Register exception handlers
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    System.Diagnostics.Debug.WriteLine($"Unhandled exception: {ex?.Message}");
                };
                Dispatcher.UIThread.UnhandledException += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"UI Thread Exception: {e.Exception.Message}"
                    );
                    Dispatcher.UIThread.Post(async () => {
                        //await MessageBoxManager.GetMessageBoxStandardWindow(
                        //        new MessageBoxStandardParams
                        //        {
                        //            ContentTitle = "Error",
                        //            ContentMessage = $"An error occurred: {e.Exception.Message}",
                        //            ButtonDefinitions = ButtonEnum.Ok,
                        //            Icon = Icon.Error,
                        //            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        //        }
                        //    )
                        //    .ShowAsync();
                    });
                    e.Handled = true;
                };

                // Handle power mode (sleep)
                var powerModeService = _serviceProvider.GetService<IPowerModeService>();
                if (powerModeService != null)
                {
                    //powerModeService.PowerModeChanged += async (sender, isSleep) =>
                    //{
                    //    if (isSleep)
                    //    {
                    //        try
                    //        {
                    //            await UpdateOnlineStatusAsync(false);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            System.Diagnostics.Debug.WriteLine(
                    //                $"Failed to update status on sleep: {ex.Message}"
                    //            );
                    //        }
                    //    }
                    //};
                    //powerModeService.StartMonitoring();
                }

                // Handle shutdown
                desktop.ShutdownRequested += async (sender, args) =>
                {
                    try
                    {
                        await UpdateOnlineStatusAsync(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Failed to update online status: {ex.Message}"
                        );
                    }
                    finally
                    {
                        MutexHandler.ReleaseMutex();
                    }
                };

                // Show login window
                desktop.MainWindow = new LoginView();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task UpdateOnlineStatusAsync(bool isOnline)
        {
            try
            {
                var userId = GlobalSetting.Instance.LoginResult?.data?.user?.id;
                var machineId = GlobalSetting.Instance.MachineId;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(machineId))
                {
                    System.Diagnostics.Debug.WriteLine("User ID or Machine ID is missing.");
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
                System.Diagnostics.Debug.WriteLine($"Failed to update online status: {ex.Message}");
                throw;
            }
        }
    }

    public static class MutexHandler
    {
        private static Mutex? _mutex;

        public static bool CreateMutex()
        {
            _mutex = new Mutex(true, "TimeTrackerMutex", out bool isNewInstance);
            return isNewInstance;
        }

        public static void ReleaseMutex()
        {
            try
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to release mutex: {ex.Message}");
            }
        }
    }
}
