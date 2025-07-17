using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sberkorus.Cbr.Domain.Exceptions.Abstractions;
using Serilog;

namespace Sberkorus.Cbr.API.Middleware
{
    /// <summary>Промежуточное ПО для обработки исключений.</summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>Инициализирует новый экземпляр middleware.</summary>
        /// <param name="next">Следующий делегат в конвейере запросов.</param>
        /// <param name="logger">Логгер для записи ошибок.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает HTTP-запрос и перехватывает исключения
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Обрабатывает исключение и формирует ответ
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса</param>
        /// <param name="exception">Исключение для обработки</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = exception switch
            {
                BusinessException businessEx => HandleBusinessException(businessEx),
                InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
                ArgumentException argEx => HandleArgumentException(argEx),
                _ => HandleGenericException(exception)
            };
            
            LogException(exception, problemDetails.Status.Value);
            context.Response.StatusCode = problemDetails.Status.Value;

            context.Response.ContentType = "application/json";

            var json = JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Обрабатывает бизнес-исключения
        /// </summary>
        /// <param name="exception">Бизнес-исключение</param>
        /// <returns>Детали проблемы для ответа</returns>
        private ProblemDetails HandleBusinessException(BusinessException exception)
        {
            return new ProblemDetails
            {
                Status = exception.StatusCode,
                Title = exception.Title,
                Detail = exception.Message,
                Type = exception.GetType().Name
            };
        }

        /// <summary>
        /// Обрабатывает InvalidOperationException (часто означает проблемы с внешними сервисами)
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <returns>Детали проблемы для ответа</returns>
        private ProblemDetails HandleInvalidOperationException(InvalidOperationException exception)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Сервис временно недоступен",
                Detail = exception.Message
            };
        }

        /// <summary>
        /// Обрабатывает ArgumentException (проблемы с параметрами)
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <returns>Детали проблемы для ответа</returns>
        private ProblemDetails HandleArgumentException(ArgumentException exception)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Некорректные параметры запроса",
                Detail = exception.Message
            };
        }

        /// <summary>
        /// Обрабатывает общие исключения
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <returns>Детали проблемы для ответа</returns>
        private ProblemDetails HandleGenericException(Exception exception)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Внутренняя ошибка сервера",
                Detail = "Произошла непредвиденная ошибка сервера"
            };
        }

        /// <summary>
        /// Логирует исключение с соответствующим уровнем
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <param name="statusCode">HTTP статус-код</param>
        private void LogException(Exception exception, int statusCode)
        {
            var message = "Обработано исключение: {ExceptionType} - {Message}";

            if (statusCode >= 500)
            {
                _logger.Error(exception, message, exception.GetType().Name, exception.Message);
            }
            else if (statusCode >= 400)
            {
                _logger.Warning(exception, message, exception.GetType().Name, exception.Message);
            }
            else
            {
                _logger.Information(message, exception.GetType().Name, exception.Message);
            }
        }
    }
}