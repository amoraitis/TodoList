using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Web.Models;

namespace TodoList.Web.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class TodosController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodosController(ITodoItemService todoItemService,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Home()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var currentDateTime = DateTime.Now;
            var calendar = new CalendarViewModel(currentDateTime.Month, currentDateTime.Year);
            
            var recentlyAddedTodos = await _todoItemService.GetRecentlyAddedItemsAsync(currentUser);
            var dueTo2daysTodos = await _todoItemService.GetDueTo2DaysItems(currentUser);
            
            var monthlyItems = await _todoItemService.GetMonthlyItems(currentUser, currentDateTime.Month);
            var calendarTodos = monthlyItems
                .Select(t => 
                    new CalendarTodoViewModel
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Content = t.Content,
                        DueToDay = t.DueTo.ToDateTimeOffset().Day
                    })
                .GroupBy(t => 
                    t.DueToDay
                )
                .ToDictionary(
                    keySelector: g => g.Key,
                    elementSelector: g => g.Select(t => t)
                );
            
            var homeViewModel = new HomeViewModel()
            {
                RecentlyAddedTodos = recentlyAddedTodos,
                CloseDueToTodos = dueTo2daysTodos,
                CalendarTodosByDay = calendarTodos,
                CalendarViewModel = calendar
            };

            return View(homeViewModel);
        }


        // GET: Todos
        public async Task<IActionResult> Index(string tag = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var todos = await _todoItemService.GetIncompleteItemsAsync(currentUser);
            var dones = await _todoItemService.GetCompleteItemsAsync(currentUser);

            if (!string.IsNullOrEmpty(tag))
            {
                todos = todos.Where(t => t.Tags.Contains(tag));
                dones = dones.Where(t => t.Tags.Contains(tag));
            }

            var model = new TodoViewModel()
            {
                Todos = todos,
                Dones = dones
            };

            return View(model);
        }

        // GET: Todos/Create
        public ViewResult Create() => View();

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,DuetoDateTime,Tags")]TodoItemCreateViewModel todo)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var todoItem = new TodoItem
            {
                Title = todo.Title,
                Content = todo.Content,
                DuetoDateTime = todo.DuetoDateTime,
                Tags = todo.Tags != null ? todo.Tags.Split(',') : new[] { "" }
            };
            var successful = await _todoItemService
                .AddItemAsync(todoItem, currentUser);

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

        [HttpPost("{todoId}")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> UploadFile([FromRoute] Guid todoId, IFormFile file)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (todoId == null || todoId == Guid.Empty)
                return BadRequest();

            if (file == null || file?.Length == 0)
                return BadRequest(typeof(IFormFile));

            var path = todoId + "//" + file.FileName;

            await _fileStorageService.CleanDirectoryAsync(todoId.ToString());

            var saved = await _fileStorageService.SaveFileAsync(path, file.OpenReadStream());

            if (!saved)
                return BadRequest("Couldn't create or replace file");

            var succeeded = await _todoItemService.SaveFileAsync(todoId, currentUser, path, file.Length);

            if (!succeeded)
                return BadRequest("Couldn't create or replace file");

            return RedirectToAction("Details", new { id = todoId });
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
                Content = todo.Content,
                Tags = todo.Tags?.Count() > 0 ? string.Join(',', todo.Tags) : ""
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
                    Content = todo.Content,
                    Tags = todo.Tags != null ? todo.Tags.Split(',') : new[] { "" }
                }, currentUser);

            if (!successful)
            {
                return BadRequest("Could not update todo.");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var todo = await _todoItemService
                .GetItemAsync(id);
            if (todo == null) return NotFound();

            ViewData["FileName"] = Path.GetFileName(todo.File.Path);
            return View(todo);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var todo = await _todoItemService.GetItemAsync(id);

            if (todo == null) return NotFound();

            var deleteViewModel = new DeleteViewModel()
            {
                Id = todo.Id,
                Title = todo.Title,
                FilePath = todo.File.Path
            };
            ViewData["FileName"] = Path.GetFileName(todo.File.Path);
            return View(deleteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, string filePath = "")
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            bool successful;
            successful = await _todoItemService
                    .DeleteTodoAsync(id, currentUser);

            if (!successful)
                return BadRequest(new { error = "Couldn't delete item!" });

            try
            {
                successful = await _fileStorageService.DeleteFileAsync(filePath, id.ToString());

                if (!successful)
                    return BadRequest(new { error = "Couldn't delete item!" });
            }
            catch (ArgumentNullException) { }
            return RedirectToAction("Index");

        }
    }
}
