using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using NWebsec.AspNetCore.Core;
using NWebsec.AspNetCore;
using NWebsec.AspNetCore.Middleware;


namespace SecureCoreApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {

            this.env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        private IHostingEnvironment env; 

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // SSL Enabled for the entire site! 
            if (!this.env.IsDevelopment())
            services.Configure<MvcOptions>(options =>
            options.Filters.Add(new RequireHttpsAttribute()));
            
            services.AddCors(c => c.AddPolicy("customer", p => p.WithOrigins("api.bank.com")));
            
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            // Step 2 
            //app.UseHsts(option => option.MaxAge(days: 30).Preload());
            
            // Step 3 Secure using CSP  
            app.UseCsp(csp => csp.DefaultSources(s =>s.Self()).
                StyleSources(css => css.Self().CustomSources("maxcdn.test.com")));
            
            // Step 4 - Prevent XFrame / Clickjacking //
            app.UseXfo(xframe => xframe.Deny());
            app.UseXfo(xframe => xframe.SameOrigin());




                
        }
    }
}
