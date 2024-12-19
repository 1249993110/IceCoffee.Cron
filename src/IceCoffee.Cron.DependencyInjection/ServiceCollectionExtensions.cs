using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IceCoffee.Cron.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<CronJobOptions> configure) where T : CronJobService
        {
            services.InternalAdd<T>(typeof(T).Name);
            services.AddOptions<CronJobOptions>(typeof(T).Name).Configure(configure).ValidateOnStart();
            return services;
        }
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, string name, Action<CronJobOptions> configure) where T : CronJobService
        {
            services.InternalAdd<T>(name);
            services.AddOptions<CronJobOptions>(name).Configure(configure).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, IConfiguration configuration) where T : CronJobService
        {
            services.InternalAdd<T>(typeof(T).Name);
            services.AddOptions<CronJobOptions>(typeof(T).Name).Bind(configuration).ValidateOnStart();
            return services;
        }
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, string name, IConfiguration configuration) where T : CronJobService
        {
            services.InternalAdd<T>(name);
            services.AddOptions<CronJobOptions>(name).Bind(configuration).ValidateOnStart();
            return services;
        }

        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, string configurationSectionPath) where T : CronJobService
        {
            services.AddOptions<CronJobOptions>(typeof(T).Name).BindConfiguration(configurationSectionPath).ValidateOnStart();
            services.InternalAdd<T>(typeof(T).Name);
            return services;
        }
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, string name, string configurationSectionPath) where T : CronJobService
        {
            services.AddOptions<CronJobOptions>(name).BindConfiguration(configurationSectionPath).ValidateOnStart();
            services.InternalAdd<T>(name);
            return services;
        }

        private static void InternalAdd<T>(this IServiceCollection services, string name) where T : CronJobService
        {
            services.TryAddSingleton<ICronDaemon, CronDaemon>();
            services.AddHostedService<CronDaemonService>();
            services.AddHostedService<T>(provider =>
            {
                var service = ActivatorUtilities.CreateInstance<T>(provider);
                service.Name = name;
                return service;
            });
        }
    }
}