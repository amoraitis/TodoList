using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Globalization;
using Npgsql;
using TodoList.Core.Contexts;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Core.Services;

namespace TodoList.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureLocalization(this IServiceCollection services)
        {
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();
        }

        public static void ConfigureSupportedCultures(this IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en-GB"),
                        new CultureInfo("el-GR"),
                        new CultureInfo("hi-IN"),
                        new CultureInfo("de-DE"),
                        new CultureInfo("hr-HR"),
                        new CultureInfo("it-IT"),
                        new CultureInfo("tr-TR"),
                        new CultureInfo("fa-IR")

                    };

                    opts.DefaultRequestCulture = new RequestCulture("en-GB");

                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;
                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                });
        }

        public static void ConfigureCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        public static void ConfigureEntityFramework(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(configuration["ConnectionStrings:PostgreSql"]);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionStringBuilder.ConnectionString, x => x.MigrationsAssembly("TodoList.Data"));
            });
        }

        public static void ConfigureSecurity(this IServiceCollection services)
        {
            // Angular's default header name for sending the XSRF token.
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        }

        public static void ConfigureSocialAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Facebook login support
            var fbAppId = config.GetSection("Authentication:Facebook:AppId").Value;
            var fbAppSecret = config.GetSection("Authentication:Facebook:AppSecret").Value;
            if (fbAppId != null && fbAppSecret != null)
            {
                services.AddAuthentication()
                    .AddFacebook(options =>
                    {
                        options.AppId = fbAppId;
                        options.AppSecret = fbAppSecret;
                    });
            }
            
            // Google login support
            var gClientId = config.GetSection("Authentication:Google:ClientId").Value;
            var gClientSecret = config.GetSection("Authentication:Google:ClientSecret").Value;
            if (gClientId != null && gClientSecret != null)
            {
                services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        options.ClientId = gClientId;
                        options.ClientSecret = gClientSecret;
                    });
            }

            // Microsoft login support
            var mClientId = config.GetSection("Authentication:Microsoft:ClientId").Value;
            var mClientSecret = config.GetSection("Authentication:Microsoft:ClientSecret").Value;
            if (mClientId != null && mClientSecret != null)
            {
                services.AddAuthentication()
                    .AddMicrosoftAccount(options =>
                    {
                        options.ClientId = mClientId;
                        options.ClientSecret = mClientSecret;
                    });
            }

            // Twitter login support
            var tConsumerKey = config.GetSection("Authentication:Twitter:ConsumerKey").Value;
            var tConsumerSecret = config.GetSection("Authentication:Twitter:ConsumerSecret").Value;
            if (tConsumerKey != null && tConsumerSecret != null)
            {
                services.AddAuthentication()
                    .AddTwitter(options =>
                    {
                        options.ConsumerKey = tConsumerKey;
                        options.ConsumerSecret = tConsumerSecret;
                    });
            }
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void ConfigureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var storageService = new LocalFileStorageService(configuration["LocalFileStorageBasePath"]);
            services.AddSingleton<IFileStorageService>(storageService);
        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddScoped<ITodoItemService, TodoItemService>();
        }

        public static void ConfigureSendGrid(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISendGridClient>(new SendGridClient(configuration["SendGrid:ServiceApiKey"]));
            services.AddTransient<SendGridMessage, SendGridMessage>();
            services.AddTransient<IEmailSender, EmailSender>();
        }
    }
}
