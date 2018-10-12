using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amoraitis.TodoList.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Amoraitis.TodoList.Services;
using System.Diagnostics;

namespace Amoraitis.TodoList.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class TodosController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodosController(ITodoItemService todoItemService,
            UserManager<ApplicationUser> userManager)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Home()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var recentlyAddedTodos = await _todoItemService.GetRecentlyAddedItemsAsync(currentUser);
            var dueTo2daysTodos = await _todoItemService.GetDueTo2DaysItems(currentUser);

            var homeViewModel = new HomeViewModel()
            {
                RecentlyAddedTodos = recentlyAddedTodos,
                CloseDueToTodos = dueTo2daysTodos
            };

            return View(homeViewModel);
        }


        // GET: Todos
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var todos = await _todoItemService.GetIncompleteItemsAsync(currentUser);
            var dones = await _todoItemService.GetCompleteItemsAsync(currentUser);
            var model = new TodoViewModel()
            {
                Todos = todos,
                Dones = dones
            };

            return View(model);
        }

        // GET: Todos/Create
        public async Task<IActionResult> Create() => View();

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem todo)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService
                .AddItemAsync(todo, currentUser);

            if (!successful)
            {
                return BadRequest(new { error = "Could not add item." });
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDone(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService
                .UpdateDoneAsync(id, currentUser);

            if (!successful)
            {
                return BadRequest("Could not update todo.");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var todo = await _todoItemService
                .GetItemAsync(id);
            if (todo == null) return NotFound();

            var editViewModel = new EditViewModel()
            {
                Id = todo.Id,
                Title = todo.Title,
                Content = todo.Content
            };
            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditViewModel todo)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var successful = await _todoItemService.UpdateTodoAsync(
                new TodoItem()
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Content = todo.Content
                }, currentUser);

            if (!successful)
            {
                return BadRequest("Could not update todo.");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var todo = await _todoItemService
                .GetItemAsync(id);
            if (todo == null) return NotFound();
            return View(todo);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var todo = await _todoItemService.GetItemAsync(id);
            var deleteViewModel = new DeleteViewModel()
            {
                Id = todo.Id,
                Title = todo.Title
            };
            return View(deleteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var successful = await _todoItemService
                .DeleteTodoAsync(id, currentUser);

            if (!successful) return BadRequest(new { error = "Couldn't delete item!" });

            return RedirectToAction("Index");

        }

        private async Task<bool> TodoExistsAsync(Guid id)
        {
            return await _todoItemService.Exists(id);
        }
    }
}
