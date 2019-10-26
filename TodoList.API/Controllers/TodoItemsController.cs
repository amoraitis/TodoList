using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoList.Core.Interfaces;

namespace TodoList.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _todoService;
        private readonly ILogger<TodoItemsController> _logger;

        public TodoItemsController(ITodoItemService todoService, ILogger<TodoItemsController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        // Get all items

        // Get done/non-done items

        // Get items by tag

        // Get item by Id

        // Create item

        // Update item

        // Delete item
    }
}