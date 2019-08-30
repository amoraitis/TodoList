using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoList.UnitTests.Resources;
using TodoList.Web;
using TodoList.Web.Controllers;
using TodoList.Web.Models;
using Xunit;

namespace TodoList.UnitTests.Controllers
{
    public class ManageUsersControllerTest
    {
        private Mock<FakeUserManager> _userManagerMock;
        private ManageUsersController _manageUsersController;

        public ManageUsersControllerTest()
        {
            _userManagerMock =  new Mock<FakeUserManager>();
            _manageUsersController = new ManageUsersController(_userManagerMock.Object);

            _manageUsersController.ControllerContext = new ControllerContext();
            _manageUsersController.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.GetUsersInRoleAsync(Constants.AdministratorRole))
                .ReturnsAsync(new ApplicationUser[] { new ApplicationUser{ Id = "1" } });

            _userManagerMock
                .Setup(manager => manager.GetUsersInRoleAsync(Constants.UserRole))
                .ReturnsAsync(new ApplicationUser[] { new ApplicationUser{ Id = "2" } });

            var result = await _manageUsersController.Index();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ManageUsersViewModel>(viewResult.Model);
        }
    }
}
