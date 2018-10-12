using Amoraitis.TodoList.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Data
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
        }

        private async static Task AddAdminRights(UserManager<ApplicationUser> userManager)
        {
            var admin = await userManager.Users
        .Where(x => x.UserName == "admin@admin.com")
        .SingleOrDefaultAsync();
         
            await userManager.AddToRoleAsync(
                admin, Constants.AdministratorRole);
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
