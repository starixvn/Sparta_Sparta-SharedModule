using System;
using System.Configuration;
using System.Runtime.Caching;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.NonWebApp.Configurations;
using ONEbook.UM.Services;
using RestSharp;

namespace ONEbook.UM.NonWebApp
{
    public static class UMDefaultInstances
    {
        private readonly static Lazy<IUMCacheService> _lazyUMCacheService = new Lazy<IUMCacheService>(() => new UMCacheService(MemoryCache.Default));
        private readonly static Lazy<IHttpContextHelper> _lazyHttpContextHelper = new Lazy<IHttpContextHelper>(() => new UMHttpContextHelper(UMCacheService));
        private readonly static Lazy<IPHelper> _lazyIPHelper = new Lazy<IPHelper>(() => new IPHelperImpl());
        private readonly static Lazy<IRestClient> _lazyHttpClient = new Lazy<IRestClient>(() => new RestClient());

        private readonly static Lazy<UMClientConfiguration> _lazyUMConfiguration = new Lazy<UMClientConfiguration>(() => new UMClientConfiguration
        {
            EnableCheckUM = UMConfigurationSection.EnableCheckUM,
            EnableUnscheduledUM = UMConfigurationSection.EnableUnscheduledUM,
            ErrorUMPageUrl = UMConfigurationSection.ErrorUMPageUrl,
            ResultCacheDuration = UMConfigurationSection.ResultCacheDuration,
            SecretKey = UMConfigurationSection.SecretKey,
            ServiceUri = UMConfigurationSection.ServiceUris,
            SiteId = UMConfigurationSection.SiteId,
            UMFrom = UMConfigurationSection.UMFrom,
            UMLoggingCategory = UMConfigurationSection.UMLoggingCategory,
            UMPageUrl = UMConfigurationSection.UMPageUrl,
            UMTo = UMConfigurationSection.UMTo
        });

        private readonly static Lazy<UMClientConfigurationSection> _lazyUMClientConfigurationSection
            = new Lazy<UMClientConfigurationSection>(() => ConfigurationManager.GetSection("ONEbook.UM") as UMClientConfigurationSection);

        private readonly static Lazy<IUMRestClient> _lazyUMClient
            = new Lazy<IUMRestClient>(() => new UMRestClient(UMConfiguration, IPHelper, HttpClient));

        private readonly static Lazy<IMaintenanceService> _lazyMaintenanceService =
            new Lazy<IMaintenanceService>(() => new MaintenanceService(UMClient, UMCacheService, HttpContextHelper, UMConfiguration));

        public static UMClientConfiguration UMConfiguration => _lazyUMConfiguration.Value;
        public static IMaintenanceService MaintenanceService => _lazyMaintenanceService.Value;
        public static IUMRestClient UMClient => _lazyUMClient.Value;
        public static IRestClient HttpClient => _lazyHttpClient.Value;
        public static IPHelper IPHelper => _lazyIPHelper.Value;
        public static IUMCacheService UMCacheService => _lazyUMCacheService.Value;
        public static IHttpContextHelper HttpContextHelper => _lazyHttpContextHelper.Value;
        internal static UMClientConfigurationSection UMConfigurationSection => _lazyUMClientConfigurationSection.Value;
    }
}