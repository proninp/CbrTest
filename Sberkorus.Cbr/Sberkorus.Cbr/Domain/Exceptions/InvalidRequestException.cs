using Sberkorus.Cbr.Domain.Exceptions.Abstractions;

namespace Sberkorus.Cbr.Domain.Exceptions
{
    /// <summary>
    /// Исключение для некорректных параметров запроса
    /// </summary>
    public class InvalidRequestException : BusinessException
    {
        public override int StatusCode => 400;
        public override string Title => "Некорректный запрос";

        /// <summary>
        /// Инициализирует исключение для некорректного запроса
        /// </summary>
        /// <param name="message">Описание ошибки</param>
        public InvalidRequestException(string message) : base(message)
        {
        }
    }
}