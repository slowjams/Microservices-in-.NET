using AspnetRunBasics.Services;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AspnetRunBasics
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
            var baseUrl = Configuration["ApiSettings:GatewayAddress"];  // localhost:8010

            services.AddTransient<LoggingDelegatingHandler>();

            services.AddHttpClient<ICatalogService, CatalogService>(c =>
                 c.BaseAddress = new Uri(baseUrl)).AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(baseUrl)).AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(baseUrl)).AddHttpMessageHandler<LoggingDelegatingHandler>();

            services.AddRazorPages();
        }

        /*
        public void ConfigureServices(IServiceCollection services)
        {
            #region database services

            //// use in-memory database
            //services.AddDbContext<AspnetRunContext>(c =>
            //    c.UseInMemoryDatabase("AspnetRunConnection"));

            var sss = Configuration.GetConnectionString("AspnetRunConnection");

            // add database dependecy
            services.AddDbContext<AspnetRunContext>(opts =>
            {
                opts.UseSqlServer(Configuration.GetConnectionString("AspnetRunConnection"));
            });

            #endregion            

            #region project services

            // add repository dependecy
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();

            #endregion

            services.AddRazorPages();
        }
        */

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
