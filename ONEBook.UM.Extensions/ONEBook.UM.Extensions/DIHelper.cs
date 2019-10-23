using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;

namespace ONEBook.UM.Extensions
{
    public static class DIHelper
    {
        /// <summary>
        /// Register UM service and its dependencies
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="requestTimeout">Request timeout for UM in second.</param>
        /// <returns></returns>
        public static IServiceCollection AddUMService(this IServiceCollection services, UMClientConfiguration umClientConfigs, int requestTimeoutInSecond = 10)
        {
            const string UMClient = "UMClient";

            services.AddSingleton(umClientConfigs);
            services.AddSingleton<IPHelper, IPHelperImpl>();
            services.AddSingleton<IHttpContextHelper, UMHttpContextHelper>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<IUMCacheService>(s => new UMCacheService(
                s.GetService<UMClientConfiguration>().SiteId.ToString(),
                s.GetService<IDistributedCache>()));
            services.AddSingleton<IUMRestClient>(s => new UMRestClient(
                UMClient, s.GetService<UMClientConfiguration>(), s.GetService<IPHelper>(), s.GetService<IHttpClientFactory>()));
            services.AddHttpClient(UMClient, (serviceProvider, client) =>
            {
                var configs = serviceProvider.GetService<UMClientConfiguration>();
                client.BaseAddress = new Uri(configs.ServiceUri);
                client.Timeout = TimeSpan.FromSeconds(requestTimeoutInSecond);
            });

            return services;
        }
    }
}