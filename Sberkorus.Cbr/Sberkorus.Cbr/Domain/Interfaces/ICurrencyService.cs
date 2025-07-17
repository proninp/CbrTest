using System;
using System.Threading;
using System.Threading.Tasks;
using Sberkorus.Cbr.Domain.Models;

namespace Sberkorus.Cbr.Domain.Interfaces
{
    /// <summary>
    /// Основной сервис для получения курсов валют
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Получить курсы валют с возможностью фильтрации
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="currencyNumCode">ISO цифровой код валюты для фильтрации (опционально)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, int? currencyNumCode = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить курсы валют с возможностью фильтрации
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="currencyCharCode">ISO символьный код валюты для фильтрации (опционально)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, string currencyCharCode = null,
            CancellationToken cancellationToken = default);
    }
}