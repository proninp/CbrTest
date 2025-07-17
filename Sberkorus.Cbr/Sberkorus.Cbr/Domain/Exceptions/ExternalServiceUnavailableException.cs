using System;
using Sberkorus.Cbr.Domain.Exceptions.Abstractions;

namespace Sberkorus.Cbr.Domain.Exceptions
{
    /// <summary>
    /// Исключение для случая недоступности внешнего сервиса
    /// </summary>
    public class ExternalServiceUnavailableException : BusinessException
    {
        public override int StatusCode => 503;
        public override string Title => "Сервис временно недоступен";

        /// <summary>
        /// Инициализирует исключение недоступности сервиса
        /// </summary>
        /// <param name="serviceName">Название сервиса</param>
        /// <param name="innerException">Внутреннее исключение</param>
        public ExternalServiceUnavailableException(string serviceName, Exception innerException = null) 
            : base($"Сервис {serviceName} временно недоступен. Повторите попытку позже.", innerException)
        {
        }
    }
}