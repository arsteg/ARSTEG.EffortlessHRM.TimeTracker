using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Services.Communication;
using TimeTracker.Services.Interfaces;
using TimeTracker.ViewModels;
using TimeTracker.ViewModels.Communication;

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

            // Register Communication services
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddSingleton<CommunicationWebSocketService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<TimeTrackerViewModel>();
            services.AddTransient<ProductivityAppsSettingsViewModel>();
            services.AddTransient<CommunicationViewModel>();

            return services;
        }
    }
}
