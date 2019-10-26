using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using TodoList.Core.Contexts;

namespace TodoList.API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configures Entity Framework DbContext for the project.
        /// </summary>
        public static void ConfigureEntityFramework(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkSqlServer().AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        public static void ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: Is it really needed
        }

        /// <summary>
        /// Configures API spec by using swagger.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "TodoList API", Version = "v1"});
            });
        }

        /// <summary>
        /// Configures JSON Web Token authentication scheme.
        /// </summary>
        public static void ConfigureJwtAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication().AddJwtBearer(); // TODO: Probably needs more work.
        }
    }
}
