using System;
using Microsoft.Extensions.DependencyInjection;

namespace TimeTracker.Services
{
    /// <summary>
    /// Provides access to the service provider for dependency injection.
    /// This is a transitional class to support the migration from MVVM Light's SimpleIoc
    /// to Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class AppServiceProvider
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the current service provider instance.
        /// </summary>
        public static IServiceProvider Instance => _serviceProvider
            ?? throw new InvalidOperationException("Service provider has not been initialized. Call Initialize() first.");

        /// <summary>
        /// Initializes the service provider with the given services.
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets a service of the specified type.
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return Instance.GetService<T>();
        }

        /// <summary>
        /// Gets a required service of the specified type. Throws if not found.
        /// </summary>
        public static T GetRequiredService<T>() where T : class
        {
            return Instance.GetRequiredService<T>();
        }
    }
}
