using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Core.Contexts;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;

namespace TodoList.Core.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClock _clock;

        public TodoItemService(ApplicationDbContext context, IClock clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task<IEnumerable<TodoItem>> GetItemsByTagAsync(ApplicationUser currentUser, string tag)
        {
            return await _context.Todos
                .Where(t => t.Tags.Contains(tag))
                .ToArrayAsync();
        }

        public async Task<bool> AddItemAsync(TodoItem todo, ApplicationUser user)
        {
            todo.Id = Guid.NewGuid();
            todo.Done = false;
            todo.Added = _clock.GetCurrentInstant();
            todo.UserId = user.Id;
            todo.File = new FileInfo
            {
                TodoId = todo.Id,
                Path = "",
                Size = 0
            };
            _context.Todos.Add(todo);

            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<IEnumerable<TodoItem>> GetIncompleteItemsAsync(ApplicationUser user)
        {
            return await _context.Todos
                .Where(t => !t.Done && t.UserId == user.Id)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetCompleteItemsAsync(ApplicationUser user)
        {
            return await _context.Todos
                .Where(t => t.Done && t.UserId == user.Id)
                .ToArrayAsync();
        }

        public bool Exists(Guid id)
        {
            return _context.Todos
                .Any(t => t.Id == id);
        }

        public async Task<bool> UpdateDoneAsync(Guid id, ApplicationUser user)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id && t.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (todo == null) return false;

            todo.Done = !todo.Done;

            var saved = await _context.SaveChangesAsync();
            return saved == 1;
        }

        public async Task<bool> UpdateTodoAsync(TodoItem editedTodo, ApplicationUser user)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == editedTodo.Id && t.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (todo == null) return false;

            todo.Title = editedTodo.Title;
            todo.Content = editedTodo.Content;
            todo.Tags = editedTodo.Tags;

            var saved = await _context.SaveChangesAsync();
            return saved == 1;
        }

        public async Task<TodoItem> GetItemAsync(Guid id)
        {
            return await _context.Todos
                .Include(t => t.File)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> DeleteTodoAsync(Guid id, ApplicationUser currentUser)
        {
            var todo = await _context.Todos
                .Include(t => t.File)
                .Where(t => t.Id == id && t.UserId == currentUser.Id)
                .SingleOrDefaultAsync();

            _context.Todos.Remove(todo);
            _context.Files.Remove(todo.File);

            var deleted = await _context.SaveChangesAsync();
            return deleted > 0;
        }

        public async Task<IEnumerable<TodoItem>> GetRecentlyAddedItemsAsync(ApplicationUser currentUser)
        {
            return await _context.Todos
                .Where(t => t.UserId == currentUser.Id && !t.Done
                && DateTime.Compare(DateTime.UtcNow.AddDays(-1), t.Added.ToDateTimeUtc()) <= 0)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetDueTo2DaysItems(ApplicationUser user)
        {
            return await _context.Todos
                .Where(t => t.UserId == user.Id && !t.Done
                && DateTime.Compare(DateTime.UtcNow.AddDays(1), t.DueTo.ToDateTimeUtc()) >= 0)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetMonthlyItems(ApplicationUser user, int month)
        {
            return await _context.Todos
                .Where(t => t.UserId == user.Id && !t.Done)
                .Where(t=>t.DueTo.ToDateTimeUtc().Month == month)
                .ToArrayAsync();
        }

        public async Task<bool> SaveFileAsync(Guid todoId, ApplicationUser currentUser, string path, long size)
        {
            var todo = await _context.Todos.Include(t => t.File)
                .Where(t => t.Id == todoId && t.UserId == currentUser.Id)
                .SingleOrDefaultAsync();

            if (todo == null) return false;

            todo.File.Path = path;
            todo.File.Size = size;
            todo.File.TodoId = todo.Id;

            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }
    }
}
