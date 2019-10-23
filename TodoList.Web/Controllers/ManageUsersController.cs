using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Core;
using TodoList.Core.Models;
using TodoList.Web.Models;

namespace TodoList.Web.Controllers
{
    [Authorize(Roles = Constants.AdministratorRole)]
    [Route("[controller]/[action]")]
    public class ManageUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            var admins = (await _userManager
                .GetUsersInRoleAsync(Constants.AdministratorRole))
                .ToArray();

            var users = (await _userManager
                .GetUsersInRoleAsync(Constants.UserRole))
                .ToArray();

            var model = new ManageUsersViewModel
            {
                Administrators = admins,
                Users = users
            };

            return View(model);
        }
    }
}