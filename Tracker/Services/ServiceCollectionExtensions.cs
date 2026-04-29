using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Services.Interfaces;
using TimeTracker.ViewModels;

namespace TimeTracker.Services
{
    /// <summary>
    /// Extension methods for configuring services in the DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services with the DI container.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register HTTP and REST services
            services.AddSingleton<IHttpProvider, HttpProviders>();
            services.AddTransient<IRestService, REST>();
            services.AddTransient<IApiService, APIService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<TimeTrackerViewModel>();
            services.AddTransient<ProductivityAppsSettingsViewModel>();

            return services;
        }
    }
}
