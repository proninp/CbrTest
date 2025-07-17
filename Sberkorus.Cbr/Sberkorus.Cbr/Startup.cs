using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Sberkorus.Cbr.API.Middleware;
using Sberkorus.Cbr.Application.Services;
using Sberkorus.Cbr.Domain.Interfaces;
using Sberkorus.Cbr.Extensions;
using Serilog;

namespace Sberkorus.Cbr
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Log.Logger);
            
            services.AddRedis(Configuration);
            services.AddCbrApiService(Configuration);
            services.AddCurrencyService(Configuration);
            
            services.AddControllers();
            services.AddSwagger();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Gateway API V1");
                c.RoutePrefix = string.Empty; // Swagger UI на корне
            });

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}