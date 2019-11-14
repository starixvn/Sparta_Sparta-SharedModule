using System.Configuration;
using ONEbook.UM.Configurations;

namespace ONEbook.UM.NonWebApp.Configurations
{
    public class UMClientConfigurationSection : ConfigurationSection
    {
        private static UMClientConfiguration _settings = null;
        private static readonly object padlock = new object();

        private const string ServiceUrisName = "ServiceUris";
        private const string EnableCheckUMName = "EnableCheckUM";
        private const string AllowedIPListName = "AllowedIPs";
        private const string SecretKeyName = "SecretKey";
        private const string EnableUnscheduledUMName = "EnableUnscheduledUM";
        private const string UMFromName = "UMFrom";
        private const string UMToName = "UMTo";
        private const string UMPageUrlName = "UMPageUrl";
        private const string SiteIdName = "SiteId";
        private const string UMResultCacheDurationName = "UMResultCacheDuration";
        private const string UMLoggingCategoryName = "UMLoggingCategory";
        private const string ErrorUMPageUrlName = "ErrorUMPageUrl";
        private const string UMTokenName = "UMToken";

        [ConfigurationProperty(ServiceUrisName, IsRequired = true)]
        public string ServiceUris => (string)base[ServiceUrisName];

        [ConfigurationProperty(EnableCheckUMName, IsRequired = false, DefaultValue = true)]
        public bool EnableCheckUM => (bool)base[EnableCheckUMName];

        [ConfigurationProperty(SecretKeyName, IsRequired = false, DefaultValue = "9yBNYA5Fnv2V58+pKLvYpFwV5BMkIh9NNTxPQk6Sbss=")]
        public string SecretKey => (string)base[SecretKeyName];

        [ConfigurationProperty(EnableUnscheduledUMName, IsRequired = false, DefaultValue = false)]
        public bool EnableUnscheduledUM => (bool)base[EnableUnscheduledUMName];

        [ConfigurationProperty(UMFromName, IsRequired = false)]
        public string UMFrom => (string)base[UMFromName];

        [ConfigurationProperty(UMToName, IsRequired = false)]
        public string UMTo => (string)base[UMToName];

        [ConfigurationProperty(UMPageUrlName, IsRequired = false, DefaultValue = "~/UserLogin/Common/Offline")]
        public string UMPageUrl => (string)base[UMPageUrlName];

        [ConfigurationProperty(SiteIdName, IsRequired = true, DefaultValue = 4120)]
        public int SiteId => (int)base[SiteIdName];

        [ConfigurationProperty(UMResultCacheDurationName, IsRequired = false, DefaultValue = 60)]
        public int ResultCacheDuration => (int)base[UMResultCacheDurationName];

        [ConfigurationProperty(UMLoggingCategoryName, IsRequired = false, DefaultValue = "ONEbook-UM")]
        public string UMLoggingCategory => (string)base[UMLoggingCategoryName];

        [ConfigurationProperty(ErrorUMPageUrlName, IsRequired = false, DefaultValue = "")]
        public string ErrorUMPageUrl => (string)base[ErrorUMPageUrlName];

        public static UMClientConfiguration Settings
        {
            get
            {
                lock (padlock)
                {
                    return _settings ?? (_settings = GetUmSettingsFromAppConfigs());
                }
            }
        }

        private static UMClientConfiguration GetUmSettingsFromAppConfigs()
        {
            var section = ConfigurationManager.GetSection("ONEbook.UM") as UMClientConfigurationSection;
            return new UMClientConfiguration
            {
                EnableCheckUM = section.EnableCheckUM,
                EnableUnscheduledUM = section.EnableUnscheduledUM,
                ErrorUMPageUrl = section.ErrorUMPageUrl,
                ResultCacheDuration = section.ResultCacheDuration,
                SecretKey = section.SecretKey,
                ServiceUri = section.ServiceUris,
                SiteId = section.SiteId,
                UMFrom = section.UMFrom,
                UMLoggingCategory = section.UMLoggingCategory,
                UMPageUrl = section.UMPageUrl,
                UMTo = section.UMTo
            };
        }
    }
}