using System;
using System.Threading;
using System.Threading.Tasks;
using Sberkorus.Cbr.Domain.Models;

namespace Sberkorus.Cbr.Domain.Interfaces
{
    /// <summary>
    /// Сервис для работы с API ЦБ РФ
    /// </summary>
    public interface ICbrService
    {
        /// <summary>
        /// Получить курсы валют на указанную дату
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}