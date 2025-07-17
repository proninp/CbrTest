using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Sberkorus.Cbr.Application.Configuration;
using Sberkorus.Cbr.Application.Services;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Infrastructure.Configuration;
using Sberkorus.Cbr.Infrastructure.Services;
using StackExchange.Redis;

namespace Sberkorus.Cbr.Extensions
{
    /// <summary>
    /// Расширения для регистрации сервисов в DI контейнер.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет Redis и сервис кэширования в контейнер зависимостей.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Обновленная коллекция сервисов.</returns>
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisOptions>(configuration.GetSection(nameof(RedisOptions)));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                var redisConfiguration = ConfigurationOptions.Parse(options.ConnectionString, true);
                redisConfiguration.Password = options.Password;

                return ConnectionMultiplexer.Connect(redisConfiguration);
            });

            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }

        /// <summary>
        /// Регистрирует сервис CBR API и HTTP-клиент с конфигурацией.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Обновленная коллекция сервисов.</returns>
        public static IServiceCollection AddCbrApiService(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CbrApiOptions>(configuration.GetSection(nameof(CbrApiOptions)));
            
            services.AddHttpClient<ICbrService, CbrService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CbrApiOptions>>().Value;
                client.BaseAddress = new Uri(settings.ServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                client.DefaultRequestHeaders.Add(settings.SoapActionHeader, settings.CurrencyRatesSoapActionValue);
            });
            return services;
        }

        /// <summary>
        /// Добавляет сервис валют в контейнер зависимостей.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Обновленная коллекция сервисов.</returns>
        public static IServiceCollection AddCurrencyService(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddScoped<ICurrencyService, CurrencyService>();
        }

        /// <summary>
        /// Добавляет поддержку Swagger в контейнер зависимостей.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <returns>Обновленная коллекция сервисов.</returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cbr Gateway API",
                    Version = "v1",
                    Description = "API для получения информации от ЦБ РФ"
                });
            });
            return services;
        }
    }
}