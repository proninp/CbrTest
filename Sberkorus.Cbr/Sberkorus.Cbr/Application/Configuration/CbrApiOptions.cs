namespace Sberkorus.Cbr.Application.Configuration
{
    /// <summary>
    /// Настройки подключения к API ЦБ РФ
    /// </summary>
    public class CbrApiOptions
    {
        /// <summary>
        /// URL сервиса ЦБ РФ
        /// </summary>
        public string ServiceUrl  { get; set; }
        
        /// <summary>
        /// Таймаут запроса в секундах
        /// </summary>
        public int TimeoutSeconds  { get; set; }
        
        /// <summary>
        /// Тип содержимого запроса
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Заголовок SOAP Action
        /// </summary>
        public string SoapActionHeader { get; set; }
        
        /// <summary>
        /// Значение SOAP Action для получения курсов валют
        /// </summary>
        public string CurrencyRatesSoapActionValue { get; set; }
    }
}