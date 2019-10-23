using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoList.Core.Models;
using TodoList.UnitTests.Resources;
using TodoList.Web.Controllers;
using TodoList.Web.Models;
using Xunit;

namespace TodoList.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private readonly Mock<FakeUserManager> _userManagerMock;
        private readonly HomeController _homeController;

        public HomeControllerTest()
        {
            _userManagerMock = new Mock<FakeUserManager>();
            _homeController = new HomeController(_userManagerMock.Object);

            _homeController.ControllerContext = new ControllerContext();
            _homeController.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WhenSucceeded()
        {
            var result = await _homeController.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsRedirectToActionResult_WhenFindAUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());

            var result = await _homeController.Index();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Home", redirectToActionResult.ActionName);
            Assert.Equal("Todos", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void About_ReturnsViewResult_WhenSucceeded()
        {
            var result = _homeController.About();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Contact_ReturnsViewResult_WhenSucceeded()
        {
            var result = _homeController.Contact();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult_WhenSucceeded()
        {
            var result = _homeController.Error();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ErrorViewModel>(viewResult.Model);
        }
    }
}
