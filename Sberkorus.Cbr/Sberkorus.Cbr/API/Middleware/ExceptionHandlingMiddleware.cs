using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        /// <summary>Обрабатывает HTTP-запрос и перехватывает исключения.</summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Возникло исключение: {Message}", exception.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Внутренняя ошибка сервера",
                    Detail = "Произошла непредвиденная ошибка сервера"
                };

                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
            }
        }
    }
}