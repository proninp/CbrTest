using Sberkorus.Cbr.Domain.Exceptions.Abstractions;

namespace Sberkorus.Cbr.Domain.Exceptions
{
    /// <summary>
    /// Исключение для ошибок валидации
    /// </summary>
    public class ValidationException : BusinessException
    {
        public override int StatusCode => 422;
        public override string Title => "Ошибка валидации";

        /// <summary>
        /// Инициализирует исключение валидации
        /// </summary>
        /// <param name="message">Описание ошибки валидации</param>
        public ValidationException(string message) : base(message)
        {
        }
    }
}