using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        Action<ResourceBuilder> buildOpenTelemetryResource = builder => {  // builder is ResourceBuilder
            builder
                .AddService("ServiceB",
                    serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
                    serviceInstanceId: Environment.MachineName)
                .Build();
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JaegerExporterOptions>(Configuration.GetSection("Jaeger"));

            services
                .AddOpenTelemetry()
                .ConfigureResource(buildOpenTelemetryResource)
                .WithTracing(builder =>
                {
                    builder
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddSource(CustomTraces.Default.Name)
                        .AddJaegerExporter()
                        .AddConsoleExporter();
                })
                .WithMetrics(builder =>
                {
                    builder
                       .AddRuntimeInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddAspNetCoreInstrumentation()
                       .AddPrometheusExporter();
                });


            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}