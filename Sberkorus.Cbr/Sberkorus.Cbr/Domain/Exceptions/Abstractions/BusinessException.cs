using System;

namespace Sberkorus.Cbr.Domain.Exceptions.Abstractions
{
    /// <summary>
    /// Базовое исключение для бизнес-логики приложения
    /// </summary>
    public abstract class BusinessException : Exception
    {
        /// <summary>
        /// HTTP статус-код для ошибки
        /// </summary>
        public abstract int StatusCode { get; }

        /// <summary>
        /// Заголовок ошибки для пользователя
        /// </summary>
        public abstract string Title { get; }

        protected BusinessException(string message) : base(message) { }
        
        protected BusinessException(string message, Exception innerException) : base(message, innerException) { }
    }
}