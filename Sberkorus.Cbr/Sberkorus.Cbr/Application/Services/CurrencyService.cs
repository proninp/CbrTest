using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Serilog;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Domain.Models;

namespace Sberkorus.Cbr.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;
        private const string CbrServiceUrl = "https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
        
        public CurrencyService(HttpClient httpClient, ICacheService cacheService, ILogger logger)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            _logger = logger;
        }
        
        public async Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, int? currencyCode = null)
        {
            var workingDate = GetWorkingDate(date);
            var cacheKey = $"currency_rates_{workingDate:yyyyMMdd}";

            _logger.Information("Requesting currency rates for date: {Date}, currency code: {CurrencyCode}", 
                workingDate, currencyCode);

            // Попытка получить из кэша
            var cachedRates = await _cacheService.GetAsync<CurrencyResponse>(cacheKey);
            if (cachedRates != null)
            {
                _logger.Information("Currency rates found in cache for date: {Date}", workingDate);
                return FilterByCurrencyCode(cachedRates, currencyCode);
            }

            try
            {
                var rates = await FetchCurrencyRatesFromCbrAsync(workingDate);
                
                // Кэширование на 1 день
                await _cacheService.SetAsync(cacheKey, rates, TimeSpan.FromDays(1));
                
                _logger.Information("Currency rates fetched and cached for date: {Date}, count: {Count}", 
                    workingDate, rates.CurrencyRates.Count);

                return FilterByCurrencyCode(rates, currencyCode);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch currency rates for date: {Date}", workingDate);
                throw new InvalidOperationException("Сервис временно недоступен", ex);
            }
        }
        
        private async Task<CurrencyResponse> FetchCurrencyRatesFromCbrAsync(DateTime date)
        {
            var soapRequest = CreateSoapRequest(date);
            var content = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");

            var response = await _httpClient.PostAsync(CbrServiceUrl, content);
            response.EnsureSuccessStatusCode();

            var xmlResponse = await response.Content.ReadAsStringAsync();
            return ParseCurrencyResponse(xmlResponse, date);
        }
        
        private string CreateSoapRequest(DateTime date)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                 xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                 xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <GetCursOnDate xmlns=""http://web.cbr.ru/"">
      <On_date>{date:yyyy-MM-dd}</On_date>
    </GetCursOnDate>
  </soap12:Body>
</soap12:Envelope>";
        }
        
        private CurrencyResponse ParseCurrencyResponse(string xmlResponse, DateTime date)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            var currencies = new List<CurrencyRate>();
            var nodes = doc.GetElementsByTagName("ValuteCursOnDate");

            foreach (XmlNode node in nodes)
            {
                var currency = new CurrencyRate
                {
                    Name = GetNodeValue(node, "Vname"),
                    Nominal = decimal.Parse(GetNodeValue(node, "Vnom"), CultureInfo.InvariantCulture),
                    Rate = decimal.Parse(GetNodeValue(node, "Vcurs"), CultureInfo.InvariantCulture),
                    Code = int.Parse(GetNodeValue(node, "Vcode")),
                    CharCode = GetNodeValue(node, "VchCode"),
                    UnitRate = double.Parse(GetNodeValue(node, "VunitRate"), CultureInfo.InvariantCulture),
                    Date = date
                };
                currencies.Add(currency);
            }

            return new CurrencyResponse
            {
                Date = date,
                CurrencyRates = currencies
            };
        }
        
        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            return parentNode.SelectSingleNode(nodeName)?.InnerText ?? string.Empty;
        }
        
        private CurrencyResponse FilterByCurrencyCode(CurrencyResponse response, int? currencyCode)
        {
            if (!currencyCode.HasValue)
                return response;

            var filteredCurrencies = response.CurrencyRates
                .Where(c => c.Code == currencyCode.Value)
                .ToList();

            return new CurrencyResponse
            {
                Date = response.Date,
                CurrencyRates = filteredCurrencies
            };
        }
        
        private DateTime GetWorkingDate(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
            }
            return date;
        }
    }
}