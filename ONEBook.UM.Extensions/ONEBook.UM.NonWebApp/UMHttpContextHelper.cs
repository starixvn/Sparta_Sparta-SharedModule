using System;
using ONEbook.UM.Helpers;

namespace ONEbook.UM.NonWebApp
{
    public class UMHttpContextHelper : IHttpContextHelper
    {
        private readonly IUMCacheService _cache;

        public UMHttpContextHelper(IUMCacheService cache) => _cache = cache;

        public string GetSession(string key) => _cache.Get<string>(key);

        public string GetValueFromQueryString(string queryKey) => UMDefaultInstances.UMConfigurationSection.UMToken;

        public void SetSession(string key, string value) => _cache.Set(key, value, TimeSpan.FromMinutes(1));
    }
}