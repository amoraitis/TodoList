using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using TodoList.Core.Models;
using TodoList.Web.Models;

namespace TodoList.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                return RedirectToAction(nameof(TodosController.Home), "Todos");
            }
            ViewData["Title"] = "Home";
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact";
            ViewData["Team"] = "TodoList Team";
            ViewData["SupportEmail"] = "contact@todolist.com";
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
