using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Amoraitis.TodoList.Data;
using Amoraitis.TodoList.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Amoraitis.TodoList.Services
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

        public async Task<TodoItem[]> GetIncompleteItemsAsync(ApplicationUser user)
        {
            return await _context.Todos
                .Where(t => !t.Done && t.UserId == user.Id)
                .ToArrayAsync();
        }

        public async Task<TodoItem[]> GetCompleteItemsAsync(ApplicationUser user)
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

            if (todo.Done)
                todo.Done = false;
            else
                todo.Done = true;

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

            var saved = await _context.SaveChangesAsync();
            return saved == 1;

        }

        public async Task<TodoItem> GetItemAsync(Guid id)
        {
            return await _context.Todos
                .Include(t=>t.File)
                .Where(t => t.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> DeleteTodoAsync(Guid id, ApplicationUser currentUser)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id && t.UserId == currentUser.Id)
                .SingleOrDefaultAsync();
            _context.Remove(todo);
            var deleted = await _context.SaveChangesAsync();
            return deleted == 1;
        }

        public async Task<TodoItem[]> GetRecentlyAddedItemsAsync(ApplicationUser currentUser)
        {
            return await _context.Todos
                .Where(t => t.UserId == currentUser.Id && !t.Done
                && DateTime.Compare(DateTime.UtcNow.AddDays(-1), t.Added.ToDateTimeUtc()) <= 0)
                .ToArrayAsync();
        }

        public async Task<TodoItem[]> GetDueTo2DaysItems(ApplicationUser user)
        {
            return await _context.Todos
                .Where(t => t.UserId == user.Id && !t.Done
                && DateTime.Compare(DateTime.UtcNow.AddDays(1), t.DueTo.ToDateTimeUtc()) >= 0)
                .ToArrayAsync();
        }

        public async Task<bool> SaveFileAsync(Guid todoId, ApplicationUser currentUser, string path, long size)
        {
            var todo = await _context.Todos.Include(t => t.File)
                .Where(t => t.Id == todoId && t.UserId == currentUser.Id)
                .SingleOrDefaultAsync();

            if (todo == null)
                return false;

            todo.File.Path = path;
            todo.File.Size = size;
            todo.File.TodoId = todo.Id;

            var changes = await _context.SaveChangesAsync();

            return changes > 0;
        }
    }
}
