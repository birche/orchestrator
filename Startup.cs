using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Microsoft.Extensions.Configuration;
using Orchestrator.Repo;
using Orchestrator.Kernel;

namespace Orchestrator
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
            .Build();
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
            services.Configure<RepoSettings>(Configuration.GetSection(nameof(RepoSettings)));

            //#if DEBUG
            //            ConfigureSwagger(services);
            //#endif
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.CreateLogger<Startup>();
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.ApplicationServices.GetService<Exec>();

            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            

            //#if DEBUG
            //            app.UseSwagger();
            //            app.UseSwaggerUi();
            //#endif
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
