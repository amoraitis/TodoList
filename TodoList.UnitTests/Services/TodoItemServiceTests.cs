using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using System;
using System.Threading.Tasks;
using TodoList.Web.Data;
using TodoList.Web.Models;
using TodoList.Web.Services;
using Xunit;

namespace TodoList.UnitTests.Services
{
    public class TodoItemServiceTests
    {
        private readonly Mock<IClock> _clockMock;

        public TodoItemServiceTests()
        {
            _clockMock = new Mock<IClock>(MockBehavior.Strict);

            _clockMock
                .Setup(clock => clock.GetCurrentInstant())
                .Returns(new Instant());
        }

        [Fact]
        public async Task AddItemAsync_ReturnsTrueIfSucceeds()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            bool success;
            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                success = await tester.AddItemAsync(todoItem, user);
            }

            Assert.True(success);
        }

        [Fact]
        public async Task AddItemAsync_SetsId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                await tester.AddItemAsync(todoItem, user);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                var result = await context.Todos
                    .SingleAsync(todo => todo.Title.Equals(todoItem.Title));

                Assert.True(result.Id.ToString().Length.Equals(36));
            }
        }

        [Fact]
        public async Task AddItemAsync_SetsDoneToFalse()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                await tester.AddItemAsync(todoItem, user);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                var result = await context.Todos
                    .SingleAsync(todo => todo.Title.Equals(todoItem.Title));

                Assert.False(result.Done);
            }
        }

        [Fact]
        public async Task AddItemAsync_UsesClock()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                await tester.AddItemAsync(todoItem, user);
            }

            _clockMock
                .Verify(clock => clock.GetCurrentInstant(), Times.Once);
        }

        [Fact]
        public async Task AddItemAsync_SetsUserId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                await tester.AddItemAsync(todoItem, user);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                var result = await context.Todos
                    .SingleAsync(todo => todo.Title.Equals(todoItem.Title));

                Assert.Equal(user.Id, result.UserId);
            }
        }

        [Fact]
        public async Task AddItemAsync_SetsDefaultValuesForFile()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var user = new ApplicationUser() { Id = Guid.NewGuid().ToString() };
            var todoItem = new TodoItem()
            {
                Title = "Cleaning"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                await tester.AddItemAsync(todoItem, user);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var tester = new TodoItemService(context, _clockMock.Object);
                var result = await context.Todos
                    .Include(todo => todo.File)
                    .SingleAsync(todo => todo.Title.Equals(todoItem.Title));

                Assert.Equal(result.Id, result.File.TodoId);
                Assert.Equal(string.Empty, result.File.Path);
                Assert.Equal(0, result.File.Size);
            }
        }
    }
}
