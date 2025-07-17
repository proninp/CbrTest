using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sberkorus.Cbr.Domain.Interfaces
{
    /// <summary>
    /// Сервис кэширования данных
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получить значение из кэша
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <returns>Значение или default(T) если не найдено</returns>
        Task<T> GetAsync<T>(string key);
        
        /// <summary>
        /// Сохранить значение в кэш
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="expiration">Время жизни в кэше</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
    }
}