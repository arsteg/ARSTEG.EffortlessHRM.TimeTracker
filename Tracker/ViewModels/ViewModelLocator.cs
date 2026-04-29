using System;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// ViewModels are resolved from the DI container.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Gets the TimeTrackerViewModel from the DI container.
        /// </summary>
        public TimeTrackerViewModel TimeTracker => AppServiceProvider.GetRequiredService<TimeTrackerViewModel>();

        /// <summary>
        /// Gets the LoginViewModel from the DI container.
        /// </summary>
        public LoginViewModel Login => AppServiceProvider.GetRequiredService<LoginViewModel>();

        /// <summary>
        /// Gets the ProductivityAppsSettingsViewModel from the DI container.
        /// </summary>
        public ProductivityAppsSettingsViewModel ProductivityAppsSettings => AppServiceProvider.GetRequiredService<ProductivityAppsSettingsViewModel>();

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            // Cleanup logic if needed
        }
    }
}
