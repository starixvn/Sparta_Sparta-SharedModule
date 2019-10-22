using System;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using ONEbook.UM.Helpers;

namespace ONEBook.UM.Extensions
{
    public class UMCacheService : IUMCacheService
    {
        private readonly string _prefix;
        private readonly IDistributedCache _cache;

        public UMCacheService(string siteId, IDistributedCache cache)
        {
            _prefix = siteId + "UMSession_";
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            var data = _cache.Get(_prefix + key);
            if (data is null)
            {
                return default(T);
            }
            var content = Encoding.UTF8.GetString(data);
            return Serializer.Deserialize<T>(content);
        }

        public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            if (!Equals(value, default(T)))
            {
                var content = Serializer.Serialize(value);
                var data = Encoding.UTF8.GetBytes(content);

                _cache.Set(_prefix + key, data, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration
                });
            }
        }
    }
}