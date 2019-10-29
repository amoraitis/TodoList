using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoList.API.Models;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;

namespace TodoList.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoItemsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITodoItemService _todoService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(UserManager<ApplicationUser> userManager, 
            ITodoItemService todoService, 
            IFileStorageService fileStorageService,
            IMapper mapper,
            ILogger<TodoItemsController> logger)
        {
            _userManager = userManager;
            _todoService = todoService;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _logger = logger;
        }

        // Get all items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetAllAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried getting all items.");
                return Unauthorized();
            }
            var items = new List<TodoItem>();
            items.AddRange(await _todoService.GetCompleteItemsAsync(user));
            items.AddRange(await _todoService.GetIncompleteItemsAsync(user));

            _logger.LogInformation($"Returned all items to {user.Email}");
            return Ok(items);
        }

        // Get done/non-done items
        [HttpGet("complete")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetCompleteItemsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried getting all complete items.");
                return Unauthorized();
            }
            var items = await _todoService.GetCompleteItemsAsync(user);

            _logger.LogInformation($"Returned all complete items to {user.Email}");
            return Ok(items);
        }

        [HttpGet("incomplete")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetIncompleteItemsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried getting all incomplete items.");
                return Unauthorized();
            }
            var items = await _todoService.GetIncompleteItemsAsync(user);

            _logger.LogInformation($"Returned all incomplete items to {user.Email}");
            return Ok(items);
        }

        // Get items by tag
        [HttpGet("bytag/{tag}")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetItemsByTag(string tag)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried getting all items with tag {tag}.");
                return Unauthorized();
            }
            var items = await _todoService.GetItemsByTagAsync(user, tag);

            _logger.LogInformation($"Returned all items with tag {tag} to {user.Email}");
            return Ok(items);
        }

        // Get item by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetItemById(Guid id)
        {
            var item = await _todoService.GetItemAsync(id);
            if(item == null)
            {
                _logger.LogError($"Item with id {id} not found.");
                return NotFound();
            }

            _logger.LogInformation($"Returned item with ID {id}");
            return Ok(item);
        }

        // Create item
        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateItem([FromBody] TodoItemDto item)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried creating an item.");
                return Unauthorized();
            }

            if (item == null)
            {
                _logger.LogError($"Received null item.");
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Received invalid item.");
                return BadRequest();
            }
            if (item.Done == null) item.Done = false;

            // create mapping
            var dbItem = _mapper.Map<TodoItem>(item);
            await _todoService.AddItemAsync(dbItem, user);

            _logger.LogInformation($"Created new TodoItem with id {dbItem.Id}");
            return CreatedAtAction(nameof(GetItemById), new { Id = dbItem.Id }, dbItem);
        }

        // Upload file
        [HttpPost("{todoId}")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> UploadFile(Guid todoId, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried creating an item.");
                return Unauthorized();
            }

            if (todoId == Guid.Empty)
            {
                _logger.LogError($"Given todoId is empty.");
                return BadRequest();
            }

            var item = await _todoService.GetItemAsync(todoId);
            if(item == null)
            {
                _logger.LogError($"Item with id {todoId} not found.");
                return NotFound();
            }

            if (file == null || file?.Length == 0)
            {
                _logger.LogError($"File is null or empty.");
                return BadRequest(typeof(IFormFile));
            }

            var path = todoId + "//" + file.FileName;

            await _fileStorageService.CleanDirectoryAsync(todoId.ToString());

            var saved = await _fileStorageService.SaveFileAsync(path, file.OpenReadStream());

            if (!saved)
                return BadRequest("Couldn't create or replace file");

            var succeeded = await _todoService.SaveFileAsync(todoId, user, path, file.Length);

            if (!succeeded)
            {
                _logger.LogError("Couldn't create or replace file.");
                return BadRequest("Couldn't create or replace file.");
            }

            return CreatedAtAction(nameof(GetItemById), new {Id = todoId}, item);
        }

        // Update item
        [HttpPut("{id}")]
        public async Task<ActionResult<TodoItem>> UpdateItemAsync([FromBody] TodoItemDto newItem, Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried creating an item.");
                return Unauthorized();
            }

            if (newItem == null)
            {
                _logger.LogError($"Received null item.");
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Received invalid item.");
                return BadRequest();
            }

            if (newItem.Done == null) newItem.Done = false;

            var dbItem = await _todoService.GetItemAsync(id);
            if (dbItem == null)
            {
                _logger.LogError($"Item with id {id} not found.");
                return NotFound();
            }

            dbItem = _mapper.Map<TodoItem>(newItem);
            if (dbItem.Done)
                await _todoService.UpdateDoneAsync(id, user);
            else
                await _todoService.UpdateTodoAsync(dbItem, user);

            _logger.LogInformation($"Updated item with id {dbItem.Id}.");
            return NoContent();
        }

        // Update status
        [HttpPatch("{id:Guid}/{status:bool}")]
        public async Task<ActionResult> UpdateStatus(Guid id, bool status)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried creating an item.");
                return Unauthorized();
            }

            var item = await _todoService.GetItemAsync(id);
            if (item == null)
            {
                _logger.LogError($"Item with id {id} not found.");
                return NotFound();
            }

            if (status)
            {
                await _todoService.UpdateDoneAsync(id, user);
            }

            _logger.LogInformation($"Item with id {id} was set to DONE.");
            return NoContent();
        }

        // Delete item
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _logger.LogError($"Unknown user tried creating an item.");
                return Unauthorized();
            }

            await _todoService.DeleteTodoAsync(id, user);

            _logger.LogInformation($"Removed item with id {id}.");
            return NoContent();
        }
    }
}