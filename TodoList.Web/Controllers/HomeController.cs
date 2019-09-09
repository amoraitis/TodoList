using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TodoList.Web.Models;

namespace TodoList.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSecrets secrets;
        private readonly string callback_uri = ""; // TODO: Specify callback uri for wunderlist

        public HomeController(UserManager<ApplicationUser> userManager, IOptions<AppSecrets> appSecretsOptions)
        {
            _userManager = userManager;
            secrets = appSecretsOptions.Value;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                return RedirectToAction(nameof(TodosController.Home), "Todos");
            }
            return View();
        }

        public async Task<IActionResult> ImportWundellist()
        {
            return Redirect($"https://www.wunderlist.com/oauth/authorize?client_id={secrets.Wunderlist.ClientId}&redirect_uri={callback_uri}&state=RANDOM");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
