using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Website;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace WebApplicationBasic
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Startup>()
                .AddApplicationInsightsSettings()
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment Environment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddApplicationInsightsTelemetry(Configuration);

            services.Configure<PcoApiClient.PcoAppRegistration>(Configuration.GetSection("PcoApp"));

            services.Configure<PcoApiClient.PcoAuthenticationOptions>(Configuration.GetSection("PcoAuth"));

            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

            services.AddMemoryCache();

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = new PathString("/Home/PcoLogin");
            });

            // Add framework services.
            services.AddMvc();

            services.AddScoped<PcoApiClient.PcoApiClient>((sp) =>
            {
                var httpContext = sp.GetService<IHttpContextAccessor>().HttpContext;

                return new PcoApiClient.PcoApiClient(new PcoApiClient.PcoApiOptions()
                {
                    AuthenticationMethod = "Bearer",
                    Password = httpContext.User.FindFirst(ClaimsExtensions.AccessToken).Value
                });
            });

            services.AddScoped<Website.Models.PcoTenant>((sp) =>
            {
                var httpContext = sp.GetService<IHttpContextAccessor>().HttpContext;
                var cache = sp.GetService<IMemoryCache>();
                var orgID = httpContext.User.FindFirst(ClaimsExtensions.OrganizationID).Value;
                var cacheKey = $"Organization:{orgID}";

                return cache.GetOrCreateAsync<Website.Models.PcoTenant>(cacheKey, async (cacheEntry) =>
                {
                    var client = sp.GetService<PcoApiClient.PcoApiClient>();
                    var orgResponse = await client.GetOrganization();

                    //TODO: Get Custom Awana data field defs and what-not

                    return new Website.Models.PcoTenant()
                    {
                        Organization = orgResponse.Data.Attributes
                    };
                }).Result;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
