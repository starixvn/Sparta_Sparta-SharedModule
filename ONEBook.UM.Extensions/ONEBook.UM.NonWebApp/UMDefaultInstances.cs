using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.NonWebApp.Configurations;
using ONEbook.UM.Services;

namespace ONEbook.UM.NonWebApp
{
    public static class UMDefaultInstances
    {
        private static MaintenanceService _instance = null;
        private static readonly object padlock = new object();

        public static MaintenanceService MaintenanceService
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new MaintenanceService(UMClient, UMCache, HttpContextHelper, UMConfiguration));
                }
            }
        }

        public static UMClientConfiguration UMConfiguration => UMClientConfigurationSection.Settings;
        public static IUMRestClient UMClient => UMRestClient.Instance;
        public static IUMCacheService UMCache => UMCacheService.Instance;
        public static IHttpContextHelper HttpContextHelper => UMHttpContextHelper.Instance;
    }
}