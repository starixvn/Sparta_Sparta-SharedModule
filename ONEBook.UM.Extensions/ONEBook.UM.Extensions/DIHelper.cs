using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;

namespace ONEBook.UM.Extensions
{
    public static class DIHelper
    {
        public static IServiceCollection AddUMService(this IServiceCollection services, UMClientConfiguration umClientConfigs, int requestTimeoutInSecond = 10)
        {
            services.AddSingleton(umClientConfigs);
            services.AddSingleton<IPHelper, IPHelperImpl>();
            services.AddSingleton<IHttpContextHelper, UMHttpContextHelper>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<IUMCacheService>(s => new UMCacheService(
                s.GetService<UMClientConfiguration>().SiteId.ToString(),
                s.GetService<IDistributedCache>()));
            services.AddSingleton<IUMRestClient, UMRestClient>();
            services.AddHttpClient(UMRestClient.UMClientName, (serviceProvider, client) =>
            {
                var configs = serviceProvider.GetService<UMClientConfiguration>();
                client.BaseAddress = new Uri(configs.ServiceUri);
                client.Timeout = TimeSpan.FromSeconds(requestTimeoutInSecond);
            });

            return services;
        }
    }
}