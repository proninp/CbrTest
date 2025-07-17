using System;
using System.Threading.Tasks;
using Serilog;
using Newtonsoft.Json;
using Sberkorus.Cbr.Domain.Interfaces;
using StackExchange.Redis;

namespace Sberkorus.Cbr.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger _logger;
        
        public RedisCacheService(IConnectionMultiplexer redis, ILogger logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }
        
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    _logger.Debug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.Debug("Cache hit for key: {Key}", key);
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting value from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var serializedValue = JsonConvert.SerializeObject(value);
                await _database.StringSetAsync(key, serializedValue, expiration);
                _logger.Debug("Value cached for key: {Key}, expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting value to cache for key: {Key}", key);
            }
        }
    }
}