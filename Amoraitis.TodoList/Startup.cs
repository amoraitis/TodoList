using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amoraitis.TodoList.Data;
using Amoraitis.TodoList.Models;
using Amoraitis.TodoList.Services;
using NodaTime;
using Amoraitis.TodoList.Services.Storage;
using Microsoft.AspNetCore.Http;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Amoraitis.TodoList
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json",false,true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Startup>()
            .Build();
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Angular's default header name for sending the XSRF token.
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (Environment.IsDevelopment())
                    options.UseSqlServer(Configuration["ConnectionStrings:DevelopementConnection"]);
                else if (Environment.IsProduction())
                    options.UseSqlServer(Configuration["ConnectionStrings:DockerEnviroment"]);
            });
                

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // Add storage service
            var storageService = new LocalFileStorageService(Configuration["LocalFileStorageBasePath"]);
            services.AddSingleton<IFileStorageService>(storageService);
            // Add Nodatime IClock
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddSingleton<SendGridClient>(new SendGridClient(Configuration["SendGrid:ServiceApiKey"]));
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<ISendGridClient>(new SendGridClient(Configuration["SendGrid:ServiceApiKey"]));
            services.AddTransient<SendGridMessage, SendGridMessage>();
            services.AddScoped<ITodoItemService, TodoItemService>();
            services.AddLogging();
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
