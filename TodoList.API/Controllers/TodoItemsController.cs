using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITodoItemService _todoService;
        private readonly IMapper _mapper;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(UserManager<ApplicationUser> userManager, 
            ITodoItemService todoService, 
            IMapper mapper,
            ILogger<TodoItemsController> logger)
        {
            _userManager = userManager;
            _todoService = todoService;
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
                _logger.LogError($"Unknown user tryied getting all items.");
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
                _logger.LogError($"Unknown user tryied getting all complete items.");
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
                _logger.LogError($"Unknown user tryied getting all incomplete items.");
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
                _logger.LogError($"Unknown user tryied getting all items with tag {tag}.");
                return Unauthorized();
            }
            var items = await _todoService.GetItemsByTagAsync(user, tag);

            _logger.LogInformation($"Returned all items with tag {tag} to {user.Email}");
            return Ok(items);
        }

        // Get item by Id
        [HttpGet("{id}}")]
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
                _logger.LogError($"Unknown user tryied creating an item.");
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

            // create mapping
            var dbItem = new TodoItem();
            _mapper.Map(item, dbItem);

            await _todoService.AddItemAsync(dbItem, user);

            _logger.LogInformation($"Created new TodoItem with id {dbItem.Id}");
            return CreatedAtAction(nameof(GetItemById), new {Id = dbItem.Id}, dbItem);
        }

        // Update item

        // Delete item
    }
}