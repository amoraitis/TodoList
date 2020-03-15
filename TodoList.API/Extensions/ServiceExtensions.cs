using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Npgsql;
using Swashbuckle.AspNetCore.Swagger;
using TodoList.API.MapperProfiles;
using TodoList.Core.Contexts;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Core.Services;

namespace TodoList.API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configures Entity Framework DbContext for the project.
        /// </summary>
        public static void ConfigureEntityFramework(this IServiceCollection services, 
            IConfiguration configuration, 
            ILogger logger)
        {   
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(configuration["ConnectionStrings:PostgreSql"]);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionStringBuilder.ConnectionString, x => x.MigrationsAssembly("TodoList.Data"));
            });
            
            logger.LogInformation("Configured EF.");
        }

        /// <summary>
        /// Configures API spec by using swagger.
        /// </summary>
        public static void ConfigureSwagger(this IServiceCollection services, ILogger logger)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "TodoList API", Version = "v1"});
            });
            logger.LogInformation("Configured Swagger.");
        }

        /// <summary>
        /// Configures JSON Web Token authentication scheme.
        /// </summary>
        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration config, ILogger logger)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = config["Authentication:JWT:Issuer"],
                        ValidAudience = config["Authentication:JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["Authentication:JWT:SecurityKey"]))
                    };
                });
            services.AddAuthorization();
            logger.LogInformation("Configured JWT authentication scheme.");
        }

        /// <summary>
        /// Configures identity services.
        /// </summary>
        public static void ConfigureIdentity(this IServiceCollection services, ILogger logger)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            logger.LogInformation("Configured Identity service.");
        }

        /// <summary>
        /// Configures repository service.
        /// </summary>
        public static void ConfigureRepository(this IServiceCollection services, ILogger logger)
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddScoped<ITodoItemService, TodoItemService>();
            logger.LogInformation("Configured Repository services.");
        }

        /// <summary>
        /// Configures AutoMapper services.
        /// </summary>
        public static void ConfigureAutoMapper(this IServiceCollection services, ILogger logger)
        {
            services.AddAutoMapper(typeof(Startup), typeof(TodoItemProfile));
            logger.LogInformation("Configured AutoMapper service.");
        }

        /// <summary>
        /// Configures file storage service.
        /// </summary>
        public static void ConfigureStorage(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            var storageService = new LocalFileStorageService(configuration["LocalFileStorageBasePath"]);
            services.AddSingleton<IFileStorageService>(storageService);
            
            logger.LogInformation("Configured Storage service.");
        }
    }
}
