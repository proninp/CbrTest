using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Newtonsoft.Json;
using Sberkorus.Cbr.Domain.Interfaces;
using StackExchange.Redis;

namespace Sberkorus.Cbr.Infrastructure.Services
{
    // <summary>
    // Сервис кэширования данных в Redis.
    // </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger _logger;

        /// <summary>Инициализирует новый экземпляр сервиса кэширования.</summary>
        /// <param name="redis">Подключение к Redis.</param>
        /// <param name="logger">Логгер.</param>
        public RedisCacheService(IConnectionMultiplexer redis, ILogger logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Получить значение из кэша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <returns>Значение или default(T) если не найдено</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    _logger.Debug("Значение не найдено в кэше по ключу: {Key}", key);
                    return default;
                }

                _logger.Debug("Значение получено из кэша по ключу: {Key}", key);
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка получения значения из кэша по ключу: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Сохранить значение в кэш
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="expiration">Время жизни в кэше</param>
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var serializedValue = JsonConvert.SerializeObject(value);
                await _database.StringSetAsync(key, serializedValue, expiration);
                _logger.Debug("Значение сохранено в кэш. Ключ: {Key}, время жизни: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка сохранения значения в кэш по ключу: {Key}", key);
            }
        }
    }
}