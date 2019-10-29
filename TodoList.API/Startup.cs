using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TodoList.API.Extensions;

namespace TodoList.API
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        public Startup(IHostingEnvironment hostingContext, ILogger<Startup> logger, IConfiguration configuration)
        {
            Configuration = configuration;

            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureEntityFramework(Configuration, _logger);
            services.ConfigureIdentity(_logger);
            services.ConfigureJwtAuthentication(Configuration, _logger);
            services.ConfigureRepository(_logger);
            services.ConfigureStorage(Configuration, _logger);
            services.ConfigureAutoMapper(_logger);
            services.ConfigureSwagger(_logger);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList Api v1");
            });

            app.UseMvc();
        }
    }
}
