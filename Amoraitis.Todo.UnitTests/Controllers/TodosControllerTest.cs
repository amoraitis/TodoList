using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Amoraitis.Todo.UnitTests.Resources;
using Amoraitis.TodoList.Controllers;
using Amoraitis.TodoList.Models;
using Amoraitis.TodoList.Services;
using Amoraitis.TodoList.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Amoraitis.Todo.UnitTests.Controllers
{
    public class TodosControllerTest
    {
        private Mock<ITodoItemService> _todoItemServiceMock;
        private Mock<IFileStorageService> _fileItemServiceMock;
        private Mock<FakeUserManager> _userManagerMock;

        private TodosController _todosController;

        public TodosControllerTest()
        {
            this._todoItemServiceMock = new Mock<ITodoItemService>();
            this._fileItemServiceMock = new Mock<IFileStorageService>();
            this._userManagerMock =  new Mock<FakeUserManager>();

            this._userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { UserName = "Max", Email = "max@example.com" });

            this._todosController = new TodosController(this._todoItemServiceMock.Object,
                        this._userManagerMock.Object,
                        this._fileItemServiceMock.Object);
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
            this._todoItemServiceMock
                .Setup(service => service.AddItemAsync(It.IsAny<TodoItem>(), It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _todosController.Create(new TodoItem {
                Id = new Guid(),
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now
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

            var result = await _todosController.Create(new TodoItem {
                Id = new Guid(),
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_WhenTodoItemIsInvalid()
        {

            _todosController.ModelState.AddModelError("Test error", "Test error");

            var result = await _todosController.Create(new TodoItem {
                Id = new Guid(),
                Title = "Test title",
                Content = "Test content",
                DuetoDateTime = DateTime.Now
            });

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
