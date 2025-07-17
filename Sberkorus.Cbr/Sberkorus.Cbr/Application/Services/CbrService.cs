using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Options;
using Sberkorus.Cbr.Application.Configuration;
using Sberkorus.Cbr.Domain.Exceptions;
using Sberkorus.Cbr.Domain.Exceptions.Abstractions;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Domain.Models;
using Sberkorus.Cbr.Extensions;
using Serilog;

namespace Sberkorus.Cbr.Application.Services
{
    /// <summary>
    /// Сервис для работы с SOAP API ЦБ РФ
    /// </summary>
    public class CbrService : ICbrService
    {
        private readonly HttpClient _httpClient;
        private readonly CbrApiOptions _options;
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса ЦБ РФ
        /// </summary>
        /// <param name="httpClient">HTTP клиент</param>
        /// <param name="options">Настройки API ЦБ РФ</param>
        /// <param name="logger">Логгер</param>
        public CbrService(HttpClient httpClient, IOptions<CbrApiOptions> options, ILogger logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Получить курсы валют на указанную дату из API ЦБ РФ
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Курсы валют</returns>
        public async Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date,
            CancellationToken cancellationToken = default)
        {
            _logger.Information("Начало получения курсов валют на дату: {Date}",
                date.ToDateString());

            try
            {
                var soapRequest = CreateSoapRequest(date);
                var content = new StringContent(soapRequest, Encoding.UTF8, _options.ContentType);

                var response = await _httpClient.PostAsync(_options.ServiceUrl, content, cancellationToken);
                _logger.Information("Получен ответ от сервиса. Статус: {StatusCode}", (int)response.StatusCode);
                response.EnsureSuccessStatusCode();

                _logger.Debug("Чтение содержимого ответа");
                var xmlResponse = await response.Content.ReadAsStringAsync();

                _logger.Debug("Парсинг ответа с курсами валют");
                var result = ParseCurrencyResponse(xmlResponse, date);

                _logger.Information("Успешно получены курсы валют на дату: {Date}. Найдено {Count} записей",
                    date.ToDateString(), result.CurrencyRates.Count);
                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceUnavailableException("ЦБ РФ", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ExternalServiceUnavailableException("ЦБ РФ (таймаут)", ex);
            }
            catch (Exception ex) when (!(ex is BusinessException))
            {
                throw new ExternalServiceUnavailableException("Непредвиденная ошибка ЦБ РФ", ex);
            }
        }

        /// <summary>
        /// Создает SOAP запрос для получения курсов валют
        /// </summary>
        /// <param name="date">Дата курса</param>
        /// <returns>XML строка SOAP запроса</returns>
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

        /// <summary>
        /// Парсит XML ответ от ЦБ РФ в объект CurrencyResponse
        /// </summary>
        /// <param name="xmlResponse">XML строка ответа</param>
        /// <param name="date">Дата курса</param>
        /// <returns>Распарсенные курсы валют</returns>
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

        /// <summary>
        /// Извлекает значение дочернего узла XML
        /// </summary>
        /// <param name="parentNode">Родительский узел</param>
        /// <param name="nodeName">Имя дочернего узла</param>
        /// <returns>Текстовое значение узла или пустая строка</returns>
        private string GetNodeValue(XmlNode parentNode, string nodeName)
        {
            return parentNode.SelectSingleNode(nodeName)?.InnerText ?? string.Empty;
        }
    }
}