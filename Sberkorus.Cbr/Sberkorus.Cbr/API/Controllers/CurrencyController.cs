using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sberkorus.Cbr.Domain.Interfaces;

namespace Sberkorus.Cbr.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger _logger;
        
        public CurrencyController(ICurrencyService currencyService, ILogger logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }
        
        /// <summary>
        /// Получить курсы валют
        /// </summary>
        /// <param name="date">Дата курса (если не передана, то используется текущая дата)</param>
        /// <param name="currencyCode">Код валюты (если не передан, то список всех валют)</param>
        /// <returns>Курсы валют в формате JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetCurrencyRates([FromQuery] DateTime? date, [FromQuery] int? currencyCode)
        {
            try
            {
                var requestDate = date ?? DateTime.Today;
                
                _logger.Information("Currency rates request: Date={Date}, CurrencyCode={CurrencyCode}", 
                    requestDate, currencyCode);

                var result = await _currencyService.GetCurrencyRatesAsync(requestDate, currencyCode);

                if (currencyCode.HasValue && !result.CurrencyRates.Any())
                {
                    _logger.Warning("Currency with code {CurrencyCode} not found for date {Date}", 
                        currencyCode, requestDate);
                    return NoContent(); // 204
                }
                return Ok(result); // 200
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex, "Service temporarily unavailable");
                return StatusCode(503, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error occurred while processing currency rates request");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}