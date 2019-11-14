using System;
using System.Runtime.Caching;
using ONEbook.UM.Helpers;

namespace ONEbook.UM.NonWebApp
{
    public class UMCacheService : IUMCacheService
    {
        private static UMCacheService _instance = null;
        private static readonly object padlock = new object();

        private readonly ObjectCache _cache;

        public UMCacheService(ObjectCache cache)
        {
            _cache = cache;
        }

        public static UMCacheService Instance
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new UMCacheService(MemoryCache.Default));
                }
            }
        }

        public T Get<T>(string key)
        {
            var data = _cache.Get(key);
            if (data is null)
            {
                return default(T);
            }
            return (T)data;
        }

        public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            if (!Equals(value, default(T)))
            {
                _cache.Set(key, value, DateTimeOffset.Now.Add(absoluteExpiration));
            }
        }
    }
}