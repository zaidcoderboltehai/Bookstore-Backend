using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bookstore.API.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;

        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string?> GetStringAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.SetAbsoluteExpiration(expiry.Value);

            await _cache.SetStringAsync(key, value, options);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            await _cache.RemoveAsync(key);
            return true;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(value))
                return null;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await SetStringAsync(key, serializedValue, expiry);
        }
    }
}
