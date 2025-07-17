using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Domain.Models;
using Sberkorus.Cbr.Extensions;

namespace Sberkorus.Cbr.Application.Services
{
    /// <summary>
    /// Основной сервис для получения курсов валют с кэшированием и фильтрацией
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly ICbrService _cbrService;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса курсов валют
        /// </summary>
        /// <param name="cbrService">Сервис ЦБ РФ</param>
        /// <param name="cacheService">Сервис кэширования</param>
        /// <param name="logger">Логгер</param>
        public CurrencyService(ICbrService cbrService, ICacheService cacheService, ILogger logger)
        {
            _cbrService = cbrService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Получить курсы валют с фильтрацией по цифровому коду валюты
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="currencyNumCode">Цифровой код валюты (ISO 4217)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        public async Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, int? currencyNumCode = null,
            CancellationToken cancellationToken = default)
        {
            _logger.Information("Запрос курсов валют на дату: {Date}, ISO цифровой код валюты: {CurrencyNumCode}",
                date.ToDateString(), currencyNumCode);

            throw new Exception("Test");
            
            var predicate = currencyNumCode.HasValue
                ? c => c.Code == currencyNumCode.Value
                : default(Func<CurrencyRate, bool>);
            
            return await GetCurrencyRatesAsync(date, predicate, cancellationToken);
        }
        
        /// <summary>
        /// Получить курсы валют с фильтрацией по символьному коду валюты
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="currencyCharCode">Символьный код валюты (ISO 4217)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        public async Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, string currencyCharCode = null,
            CancellationToken cancellationToken = default)
        {
            _logger.Information("Запрос курсов валют на дату: {Date}, ISO символьный код валюты: {CurrencyCharCode}",
                date.ToDateString(), currencyCharCode);

            var predicate = currencyCharCode != null
                ? c => c.CharCode == currencyCharCode
                : default(Func<CurrencyRate, bool>);
            
            return await GetCurrencyRatesAsync(date, predicate, cancellationToken);
        }
        
        /// <summary>
        /// Получить курсы валют с кэшированием и применением пользовательского предиката
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="predicate">Предикат для фильтрации валют</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        private async Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, Func<CurrencyRate, bool> predicate = null,
            CancellationToken cancellationToken = default)
        {
            var workingDate = GetWorkingDate(date);
            if (workingDate != date)
            {
                _logger.Information("Дата запроса курса валюты была изменена на предыдущий рабочий день: {Date}", workingDate);
            }
            var cacheKey = GetCacheKey(workingDate);

            var cachedRates = await _cacheService.GetAsync<CurrencyResponse>(cacheKey);
            if (cachedRates != null)
            {
                _logger.Information("Курсы валют найдены в кэше для даты: {Date}", workingDate);
                return FilterResponse(cachedRates, predicate);
            }

            try
            {
                var rates = await _cbrService.GetCurrencyRatesAsync(date, cancellationToken);

                await _cacheService.SetAsync(cacheKey, rates, TimeSpan.FromDays(1));

                _logger.Information("Курсы валют успешно получены и сохранены в кэш. Дата: {Date}, количество: {Count}",
                    workingDate, rates.CurrencyRates.Count);

                return FilterResponse(rates, predicate); 
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при получении курсов валют на дату: {Date}", workingDate);
                throw;
            }
        }

        /// <summary>
        /// Фильтрует ответ с курсами валют по заданному предикату
        /// </summary>
        /// <param name="response">Ответ с курсами валют</param>
        /// <param name="predicate">Предикат для фильтрации</param>
        /// <returns>Отфильтрованный ответ</returns>
        private CurrencyResponse FilterResponse(CurrencyResponse response, Func<CurrencyRate, bool> predicate)
        {
            if (predicate == null)
                return response;
                
            var filteredCurrencies = response.CurrencyRates
                .Where(predicate)
                .ToList();

            return new CurrencyResponse
            {
                Date = response.Date,
                CurrencyRates = filteredCurrencies
            };
        }

        /// <summary>
        /// Преобразует дату к рабочему дню (исключает выходные)
        /// </summary>
        /// <param name="date">Исходная дата</param>
        /// <returns>Ближайший предыдущий рабочий день</returns>
        private DateTime GetWorkingDate(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
            }

            return date;
        }
        
        /// <summary>
        /// Формирует ключ для кэширования курсов валют
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <returns>Ключ кэша</returns>
        private string GetCacheKey(DateTime date) =>
            $"currency_rates_{date:yyyyMMdd}";
    }
}