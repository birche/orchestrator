using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Microsoft.Extensions.Configuration;
using process_tracker.Repo;
using process_tracker.Kernel;

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
            services.AddSingleton<IApplicationRepository, ApplicationRepository>();

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


#if DEBUG
            ConfigureSwagger(services);
#endif



        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.CreateLogger<Startup>();
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUi();
#endif
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Beijer exec",
                    Description = "A simple api to serve the exec orchestrator",
                    TermsOfService = "NA"
                });
                options.DescribeAllEnumsAsStrings();
            });
        }



    }
}
