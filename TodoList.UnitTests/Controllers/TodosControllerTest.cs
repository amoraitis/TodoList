using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.UnitTests.Resources;
using TodoList.Web.Controllers;
using TodoList.Web.Models;
using Xunit;

namespace TodoList.UnitTests.Controllers
{
    public class TodosControllerTest
    {
        private readonly Mock<ITodoItemService> _todoItemServiceMock;
        private readonly Mock<IFileStorageService> _fileItemServiceMock;
        private readonly Mock<FakeUserManager> _userManagerMock;

        private readonly TodosController _todosController;

        public TodosControllerTest()
        {
            _todoItemServiceMock = new Mock<ITodoItemService>();
            _fileItemServiceMock = new Mock<IFileStorageService>();
            _userManagerMock = new Mock<FakeUserManager>();

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { UserName = "Max", Email = "max@example.com" });

            _todosController = new TodosController(_todoItemServiceMock.Object,
                        _userManagerMock.Object,
                        _fileItemServiceMock.Object);
        }

        [Fact]
        public async Task Home_ReturnsViewResult_WithRecenttlyAddedAndCloseTodos()
        {
            var result = await _todosController.Home();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<HomeViewModel>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task Home_ReturnsChallengeResult_WhenUserIsNull()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.Home();

            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithAListOfTodosAndDones()
        {
            var result = await _todosController.Index();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<TodoViewModel>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task IndexWithTag_ReturnsViewResult_WithAListOfTaggedTodosAndDones()
        {
            var result = await _todosController.Index("test");
            var viewResult = Assert.IsType<ViewResult>(result);

            var allItems = Assert.IsAssignableFrom<TodoViewModel>(viewResult.ViewData.Model);
            Assert.All(allItems.Todos, i => ((List<string>) i.Tags).Contains("test"));
            Assert.All(allItems.Dones, i => ((List<string>)i.Tags).Contains("test"));
        }

        [Fact]
        public async Task Index_ReturnsChallengeResult_WhenUserIsNull()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.Index();

            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            var result = _todosController.Create();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_WhenTodoItemIsCreated()
        {
            _todoItemServiceMock
                .Setup(service => service.AddItemAsync(It.IsAny<TodoItem>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _todosController.Create(new TodoItemCreateViewModel
            {
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now,
                Tags = "test"
            });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task Create_ReturnsBadRequestObjectResult_WhenTodoItemIsNotCreated()
        {
            this._todoItemServiceMock
                .Setup(service => service.AddItemAsync(It.IsAny<TodoItem>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);

            var result = await _todosController.Create(new TodoItemCreateViewModel
            {
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now,
                Tags = "test"
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_WhenTodoItemIsInvalid()
        {
            _todosController.ModelState.AddModelError("Test error", "Test error");

            var result = await _todosController.Create(new TodoItemCreateViewModel
            {
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now,
                Tags = "test"
            });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_WhenTodoItemHasTooManyTags()
        {
            _todosController.ModelState.AddModelError("Test error", "Test error");

            var result = await _todosController.Create(new TodoItemCreateViewModel
            {
                Title = "Test title",
                Content = "Test content 1234567890",
                DuetoDateTime = DateTime.Now,
                Tags = "t1,t2,t3,t4"
            });

            Assert.False(_todosController.ModelState.IsValid);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task UpdateDone_RedirectsToIndex_WhenTodoItemIsUpdated()
        {
            this._todoItemServiceMock
                .Setup(service => service.UpdateDoneAsync(It.IsAny<Guid>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _todosController.UpdateDone(Guid.NewGuid());

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task UpdateDone_RedirectsToIndex_WhenGuidIsEmpty()
        {
            var result = await _todosController.UpdateDone(Guid.Empty);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task UpdateDone_ReturnsChallengeResult_WhenUserIsNull()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.UpdateDone(Guid.NewGuid());

            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public async Task UpdateDone_ReturnsBadRequestObjectResult_WhenTodoItemCanNotBeUpdated()
        {
            this._todoItemServiceMock
                .Setup(service => service.UpdateDoneAsync(It.IsAny<Guid>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);

            var result = await _todosController.UpdateDone(Guid.NewGuid());

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Could not update todo.", (result as BadRequestObjectResult).Value.ToString());
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WhenValidGuid()
        {
            this._todoItemServiceMock
                .Setup(service => service.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new TodoItem());

            var result = await _todosController.Edit(Guid.NewGuid());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<EditViewModel>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task Edit_ReturnsNotFoundResult_WhenGuidIsEmpty()
        {
            var result = await _todosController.Edit(Guid.Empty);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsNotFoundResult_WhenTodoItemCanNotBeFound()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.Edit(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostEdit_RedirectsToIndex_WhenTodoItemIsUpdated()
        {
            this._todoItemServiceMock
                .Setup(service => service.UpdateTodoAsync(It.IsAny<TodoItem>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _todosController.Edit(Guid.NewGuid(), new EditViewModel());

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostEdit_ReturnsNotFoundResult_WhenGuidIsEmpty()
        {
            var result = await _todosController.Edit(Guid.Empty, new EditViewModel());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostEdit_ReturnsBadRequestObjectResult_WhenTodoCanNotBeUpdated()
        {
            this._todoItemServiceMock
                .Setup(service => service.UpdateTodoAsync(It.IsAny<TodoItem>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);

            var result = await _todosController.Edit(Guid.NewGuid(), new EditViewModel());

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Could not update todo.", (result as BadRequestObjectResult).Value.ToString());
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WhenGuidIsValid()
        {
            TodoItem todo = new TodoItem();
            todo.File = new FileInfo();
            todo.File.Path = "/fake/path/fake_file.txt";

            this._todoItemServiceMock
                .Setup(service => service.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(todo);

            var result = await _todosController.Details(Guid.NewGuid());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("fake_file.txt", viewResult.ViewData["Filename"]);
        }

        [Fact]
        public async Task Details_ReturnsNotFoundResult_WhenGuidIsEmpty()
        {
            var result = await _todosController.Details(Guid.Empty);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFoundResult_WhenTodoItemCanNotBeFound()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.Details(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsViewResult_WhenGuidIsValid()
        {
            TodoItem todo = new TodoItem();
            todo.File = new FileInfo();
            todo.File.Path = "/fake/path/fake_file.txt";

            this._todoItemServiceMock
                .Setup(service => service.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(todo);

            var result = await _todosController.Delete(Guid.NewGuid());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<DeleteViewModel>(viewResult.ViewData.Model);
            Assert.Equal("fake_file.txt", viewResult.ViewData["Filename"]);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundResult_WhenGuidIsEmpty()
        {
            var result = await _todosController.Delete(Guid.Empty);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFoundResult_WhenTodoItemCanNotBeFound()
        {
            SetupGetUserAsyncToNull();

            var result = await _todosController.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToActionResult_WhenTodoItemIsDeleted()
        {
            this._todoItemServiceMock
                .Setup(service => service.DeleteTodoAsync(It.IsAny<Guid>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            this._fileItemServiceMock
                .Setup(service => service.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _todosController.DeleteConfirmed(Guid.NewGuid(), "");

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsNotFoundResult_WhenGuidIsEmpty()
        {
            var result = await _todosController.DeleteConfirmed(Guid.Empty, "");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsBadRequestObjectResult_WhenCanNotDeleteTodoItem()
        {
            this._todoItemServiceMock
                .Setup(service => service.DeleteTodoAsync(It.IsAny<Guid>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);

            var result = await _todosController.DeleteConfirmed(Guid.NewGuid(), "");

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(
                new { error = "Couldn't delete item!" }.ToString(),
                (result as BadRequestObjectResult).Value.ToString()
            );
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsBadRequestObjectResult_WhenCanNotDeleteFile()
        {
            this._todoItemServiceMock
                .Setup(service => service.DeleteTodoAsync(It.IsAny<Guid>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            this._fileItemServiceMock
                .Setup(service => service.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _todosController.DeleteConfirmed(Guid.NewGuid(), "");

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(
                new { error = "Couldn't delete item!" }.ToString(),
                (result as BadRequestObjectResult).Value.ToString()
            );
        }

        private void SetupGetUserAsyncToNull()
        {
            this._userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(() => null);
        }
    }
}
