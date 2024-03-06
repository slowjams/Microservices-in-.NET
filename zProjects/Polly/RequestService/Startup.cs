using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using RequestService.Controllers;
using System;
using System.Net.Http;

namespace RequestService
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
            //services.AddSingleton<ClientPolicy>(new ClientPolicy());

            services.AddTransient<TimingHandler>();
            services.AddTransient<ValidateHeaderHandler>();

            // Named clients
            services.AddHttpClient("GitHubClient", c =>
            {
                c.BaseAddress = new Uri("https://api.github.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            })
            .AddHttpMessageHandler<TimingHandler>()
            .AddHttpMessageHandler<ValidateHeaderHandler>();
            //

            // Typed clients
            services.AddHttpClient<MyGitHubClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactoryTesting");
            })
            .AddHttpMessageHandler<TimingHandler>()
            .AddHttpMessageHandler<ValidateHeaderHandler>();
            //

            /* there is another kind of usage in asp.net microservices project
            services
                .AddHttpClient<ICatalogService, CatalogService>(c => c.BaseAddress = new Uri(Configuration["ApiSettings:CatalogUrl"]))
            */

            // using Polly
            services.AddHttpClient("GitHubClient").AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)));
            //

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    } 
}
