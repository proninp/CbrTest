using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Infrastructure.Services;
using StackExchange.Redis;

namespace Sberkorus.Cbr.Infrastructure.Configuration
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                var configuration = ConfigurationOptions.Parse(options.ConnectionString, true);
                configuration.Password = options.Password;

                return ConnectionMultiplexer.Connect(configuration);
            });

            services
                .AddTransient<ICacheService, RedisCacheService>();

            return services;
        }
    }
}