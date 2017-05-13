﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace process_tracker
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Exec>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy => policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddMvc();
            services.AddOptions();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.CreateLogger<Startup>();
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();
        }



    }
}
