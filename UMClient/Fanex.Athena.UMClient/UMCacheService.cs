using System;
using Microsoft.Extensions.Caching.Distributed;
using MsgPack.Serialization;
using ONEbook.UM.Helpers;

namespace Fanex.Athena.UMClient
{
    public class UMCacheService : IUMCacheService
    {
        private readonly string _prefix;
        private readonly IDistributedCache _cache;

        private static readonly SerializationContext _msgPackSerializationContext = new SerializationContext
        {
            DefaultDateTimeConversionMethod = DateTimeConversionMethod.Native
        };

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

            var serializer = MessagePackSerializer.Get<T>(_msgPackSerializationContext);
            return serializer.UnpackSingleObject(data);
        }

        public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
        {
            if (!Equals(value, default(T)))
            {
                var serializer = MessagePackSerializer.Get<T>(_msgPackSerializationContext);
                var data = serializer.PackSingleObject(value);

                _cache.Set(_prefix + key, data, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration
                });
            }
        }
    }
}