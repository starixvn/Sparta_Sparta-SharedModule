using System;
using ONEbook.UM.Helpers;

namespace ONEbook.UM.NonWebApp
{
    public class UMHttpContextHelper : IHttpContextHelper
    {
        private static UMHttpContextHelper _instance = null;
        private static readonly object padlock = new object();

        private readonly IUMCacheService _cache;

        public UMHttpContextHelper(IUMCacheService cache) => _cache = cache;

        public static UMHttpContextHelper Instance
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new UMHttpContextHelper(UMCacheService.Instance));
                }
            }
        }

        public string GetSession(string key) => _cache.Get<string>(key);

        public string GetValueFromQueryString(string queryKey) => string.Empty;

        public void SetSession(string key, string value) => _cache.Set(key, value, TimeSpan.FromMinutes(1));
    }
}