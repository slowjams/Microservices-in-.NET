using Catalog.API.Data;
using Catalog.API.Repositories;
using HealthChecks.MongoDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
            });

            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();

            services
                .AddHealthChecks()
                // if uncomment these two lines, the health status will be "Unhealthy", since in term of failure's priority, "Unhealthy" > "Degraded" > "Healthy" 
                //.AddCheck("Foo", () => HealthCheckResult.Degraded("Foo is Slow!"), tags: new[] { "foo_tag" })
                //.AddCheck("Bar", () => HealthCheckResult.Unhealthy("Bar is Down!"), tags: new[] { "bar_tag" })
                .AddMongoDb(
                    Configuration["DatabaseSettings:ConnectionString"],
                    "Catalog MongoDb Health",
                    HealthStatus.Degraded);  // failureStatus

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    // Predicate = (hc) => hc.Tags.Contains("foo_tag") || hc.Tags.Contains("baz_tag")
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse  // write json output as below:
                    /*
                     {
                       "status": "Healthy",
                       "totalDuration": "00:00:00.4817021",
                       "entries": {
                         "Catalog MongoDb Health": {
                           "data": {},
                           "duration": "00:00:00.3926339",
                           "status": "Healthy",
                           "tags": []
                         }
                       }
                     }
                    */
                });
            });
        }
    }
}
