using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sberkorus.Cbr.Domain.Interfaces;

namespace Sberkorus.Cbr.API.Controllers
{
    /// <summary>
    /// Контроллер для получения курсов валют
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера курсов валют
        /// </summary>
        /// <param name="currencyService">Сервис курсов валют</param>
        /// <param name="logger">Логгер</param>
        public CurrencyController(ICurrencyService currencyService, ILogger logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Получить курсы валют с фильтрацией по цифровому коду валюты
        /// </summary>
        /// <param name="date">Дата курса (если не указана, используется текущая дата)</param>
        /// <param name="currencyCode">Цифровой код валюты для фильтрации (опционально)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют в формате JSON</returns>
        /// <response code="200">Курсы валют успешно получены</response>
        /// <response code="204">Валюта с указанным кодом не найдена</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("byNumCode")]
        public async Task<IActionResult> GetCurrencyRates([FromQuery] DateTime? date, [FromQuery] int? currencyCode,
            CancellationToken cancellationToken = default)
        {
            var requestDate = date ?? DateTime.Today;

            _logger.Information("Запрос курсов валют: Дата={Date}, Цифровой код валюты={CurrencyCode}",
                requestDate, currencyCode);

            var result = await _currencyService.GetCurrencyRatesAsync(requestDate, currencyCode, cancellationToken);

            if (currencyCode.HasValue && !result.CurrencyRates.Any())
            {
                _logger.Warning("Валюта с цифровым кодом {CurrencyCode} не найдена на дату {Date}",
                    currencyCode, requestDate);
                return NoContent(); // 204
            }

            _logger.Information("Успешно возвращены курсы валют на дату {Date}. Количество записей: {Count}",
                requestDate, result.CurrencyRates.Count);

            return Ok(result); // 200
        }

        /// <summary>
        /// Получить курсы валют с фильтрацией по символьному коду валюты
        /// </summary>
        /// <param name="date">Дата курса (если не указана, используется текущая дата)</param>
        /// <param name="currencyCharCode">Символьный код валюты для фильтрации (например, USD, EUR)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют в формате JSON</returns>
        /// <response code="200">Курсы валют успешно получены</response>
        /// <response code="204">Валюта с указанным кодом не найдена</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("byCharCode")]
        public async Task<IActionResult> GetCurrencyRatesByCharCode([FromQuery] DateTime? date,
            [FromQuery] string currencyCharCode,
            CancellationToken cancellationToken = default)
        {
            var requestDate = date ?? DateTime.Today;

            _logger.Information("Запрос курсов валют: Дата={Date}, Символьный код валюты={CurrencyCharCode}",
                requestDate, currencyCharCode);

            var result = await _currencyService.GetCurrencyRatesAsync(requestDate, currencyCharCode, cancellationToken);

            if (!string.IsNullOrEmpty(currencyCharCode) && !result.CurrencyRates.Any())
            {
                _logger.Warning("Валюта с символьным кодом {CurrencyCharCode} не найдена на дату {Date}",
                    currencyCharCode, requestDate);
                return NoContent(); // 204
            }

            _logger.Information("Успешно возвращены курсы валют на дату {Date}. Количество записей: {Count}",
                requestDate, result.CurrencyRates.Count);

            return Ok(result); // 200
        }
    }
}