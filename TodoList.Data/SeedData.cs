using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TodoList.Core;
using TodoList.Core.Models;

namespace TodoList.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var roleManager = services
                .GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRolesAsync(roleManager);
            var userManager = services
                 .GetRequiredService<UserManager<ApplicationUser>>();
            await AddAdminRights(userManager);
        }

        private static async Task AddAdminRights(UserManager<ApplicationUser> userManager)
        {
            var adminExists = userManager.Users
                .Any(x => x.UserName == "admin@admin.local");

            if (!adminExists)
            {
                var admin = new ApplicationUser
                {
                    Email = "admin@admin.local",
                    UserName = "admin@admin.local"
                };
                await userManager.CreateAsync(admin, "Admin1!");
                await userManager.AddToRoleAsync(
                    admin, Constants.AdministratorRole);
            }
        }

        private static async Task EnsureRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            var adminRoleAlreadyExists = await roleManager
                .RoleExistsAsync(Constants.AdministratorRole);

            var userRoleAlreadyExists = await roleManager
                .RoleExistsAsync(Constants.UserRole);

            if (!adminRoleAlreadyExists)
                await roleManager.CreateAsync(
                    new IdentityRole(Constants.AdministratorRole));

            if (!userRoleAlreadyExists)
                await roleManager.CreateAsync(
                    new IdentityRole(Constants.UserRole));
        }
    }
}
