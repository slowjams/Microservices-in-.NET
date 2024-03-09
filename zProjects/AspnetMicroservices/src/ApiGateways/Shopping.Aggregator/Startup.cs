using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using Shopping.Aggregator.Services;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace Shopping.Aggregator
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
            services.AddTransient<LoggingDelegatingHandler>();

            // AddHttpClient registers IHttpClientFactory and ICatalogService, CatalogService etc
            // AddTransientHttpErrorPolicy registers PolicyHttpMessageHandler (which is DelegatingHandler)
            services
                .AddHttpClient<ICatalogService, CatalogService>(c => c.BaseAddress = new Uri(Configuration["ApiSettings:CatalogUrl"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(GetCombinedPolicy());
            // v1, AddTransientHttpErrorPolicy adds a PolicyHttpMessageHandler internally
            /*
            services
                .AddHttpClient<IBasketService, BasketService>(c => c.BaseAddress = new Uri(Configuration["ApiSettings:BasketUrl"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(2)))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30)));
            */
            //

            // v2
            services
               .AddHttpClient<IBasketService, BasketService>(c =>
               {
                   c.BaseAddress = new Uri(Configuration["ApiSettings:BasketUrl"]);
               })
               .AddPolicyHandler(GetCombinedPolicy());
            // 

            services
                .AddHttpClient<IOrderService, OrderService>(c => c.BaseAddress = new Uri(Configuration["ApiSettings:OrderingUrl"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(GetCombinedPolicy());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping.Aggregator", Version = "v1" });
            });

            services
                .AddHealthChecks()
                .AddUrlGroup(new Uri($"{Configuration["ApiSettings:CatalogUrl"]}/hc"), "Catalog.API", HealthStatus.Degraded)
                .AddUrlGroup(new Uri($"{Configuration["ApiSettings:BasketUrl"]}/hc"), "Basket.API", HealthStatus.Degraded)
                .AddUrlGroup(new Uri($"{Configuration["ApiSettings:OrderingUrl"]}/hc"), "Ordering.API", HealthStatus.Degraded);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(Math.Pow(2, _)));

            return Policy.WrapAsync(retry, timeout);
        }

        // doesn't work, not sure why
        //private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        //{
        //    return
        //        HttpPolicyExtensions
        //            .HandleTransientHttpError()
        //            .WaitAndRetryAsync(
        //                retryCount: 3,
        //                sleepDurationProvider: currNthRetry => TimeSpan.FromSeconds(Math.Pow(2, currNthRetry)),
        //                onRetry: (exception, retryCount, context) =>
        //                {
        //                    Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
        //                });

        //}

        //private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        //{
        //    return
        //        HttpPolicyExtensions
        //            .HandleTransientHttpError()
        //            .CircuitBreakerAsync(
        //                handledEventsAllowedBeforeBreaking: 5,
        //                durationOfBreak: TimeSpan.FromSeconds(30));

        //}

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping.Aggregator v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc");
            });
        }
    }
}