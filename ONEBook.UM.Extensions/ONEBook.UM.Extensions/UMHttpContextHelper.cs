using System;
using Microsoft.AspNetCore.Http;
using ONEbook.UM.Helpers;

namespace ONEbook.UM.Extensions
{
    public class UMHttpContextHelper : IHttpContextHelper
    {
        private readonly IUMCacheService _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UMHttpContextHelper(IUMCacheService cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetSession(string key) => _cache.Get<string>(key);

        public string GetValueFromQueryString(string queryKey)
        {
            var context = _httpContextAccessor.HttpContext;
            var request = context.Request;
            return request.Query[queryKey];
        }

        public void SetSession(string key, string value) => _cache.Set(key, value, TimeSpan.FromMinutes(1));
    }
}