using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TodoList.Core.Models;
using TodoList.UnitTests.Resources;
using TodoList.Web.Controllers;
using TodoList.Web.Models.ManageViewModels;
using Xunit;

namespace TodoList.UnitTests.Controllers
{
    public class ManageControllerTest
    {
        private readonly Mock<FakeUserManager> _userManagerMock;
        private readonly Mock<FakeSignInManager> _signInManagerMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<UrlEncoder> _urlEncoderMock;
        private readonly Mock<ILogger<ManageController>> _loggerMock;
        private readonly ManageController _manageController;

        public ManageControllerTest()
        {
            _userManagerMock = new Mock<FakeUserManager>();
            _signInManagerMock = new Mock<FakeSignInManager>();
            _emailSenderMock = new Mock<IEmailSender>();
            _urlEncoderMock = new Mock<UrlEncoder>();
            _loggerMock = new Mock<ILogger<ManageController>>();

            _manageController = new ManageController(_userManagerMock.Object,
                    _signInManagerMock.Object,
                    _emailSenderMock.Object,
                    _loggerMock.Object,
                    _urlEncoderMock.Object);

            _manageController.ControllerContext = new ControllerContext();
            _manageController.ControllerContext.HttpContext = new DefaultHttpContext();

            var authManager = new Mock<IAuthenticationService>();

            authManager.Setup(service => service.SignOutAsync(It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult(true));

            var servicesMock = new Mock<IServiceProvider>();

            servicesMock.Setup(serviceProvider => serviceProvider.GetService(typeof(IAuthenticationService)))
                .Returns(authManager.Object);
            servicesMock.Setup(serviceProvider => serviceProvider.GetService(typeof(IUrlHelperFactory)))
                .Returns(new UrlHelperFactory());
            servicesMock.Setup(serviceProvider => serviceProvider.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(new TempDataDictionaryFactory(new SessionStateTempDataProvider()));
            servicesMock.Setup(serviceProvider => serviceProvider.GetService(typeof(IPrincipal)))
                .Returns(new ClaimsPrincipal());

            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperMock.Setup(urlHelper => urlHelper.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://fakeurl.example");

            _manageController.ControllerContext.HttpContext.RequestServices = servicesMock.Object;
            _manageController.ControllerContext.HttpContext.Session = Mock.Of<ISession>();
            _manageController.Url = urlHelperMock.Object;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            var result = await _manageController.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<IndexViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Index());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostIndex_ReturnsRedirectToActionResult_WhenSucceed()
        {
            var user = new ApplicationUser { Email = "max@example.com", PhoneNumber = "1-800-555-5555" };
            var model = new IndexViewModel { Email = "newmail@example.com", PhoneNumber = "1-800-777-7777" };

            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(manager => manager.SetEmailAsync(user, model.Email))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(manager => manager.SetPhoneNumberAsync(user, model.PhoneNumber))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _manageController.Index(model);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Your profile has been updated", _manageController.StatusMessage);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostIndex_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _manageController.ModelState.AddModelError("Test error", "Test error");

            var result = await _manageController.Index(new IndexViewModel());

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<IndexViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task PostIndex_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Index(new IndexViewModel()));

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostIndex_ThrowsApplicationException_WhenCanNotSetEmail()
        {
            var user = new ApplicationUser { Email = "max@example.com", Id = "1" };
            var model = new IndexViewModel { Email = "newmail@example.com" };

            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(manager => manager.SetEmailAsync(user, model.Email))
                .ReturnsAsync(IdentityResult.Failed());

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Index(model));

            Assert.Equal("Unexpected error occurred setting email for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostIndex_ThrowsApplicationException_WhenCanNotSetPhoneNumber()
        {
            var user = new ApplicationUser { Email = "max@example.com", PhoneNumber = "1-800-555-5555", Id = "1" };
            var model = new IndexViewModel { Email = "max@example.com", PhoneNumber = "1-800-777-7777" };

            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(manager => manager.SetPhoneNumberAsync(user, model.PhoneNumber))
                .ReturnsAsync(IdentityResult.Failed());

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Index(model));

            Assert.Equal("Unexpected error occurred setting phone number for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.HasPasswordAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _manageController.ChangePassword();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task ChangePassword_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.ChangePassword());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsRedirectToActionResult_WhenUserHasNoPassword()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.HasPasswordAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);

            var result = await _manageController.ChangePassword();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("SetPassword", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostChangePassword_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            var model = new ChangePasswordViewModel { OldPassword = "1234", NewPassword = "abcDEF123_" };

            SetGetUserAsyncMethod();

            _signInManagerMock
                .Setup(manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            _userManagerMock
                .Setup(manager => manager.ChangePasswordAsync(It.IsAny<ApplicationUser>(), model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _manageController.ChangePassword(model);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Your password has been changed.", _manageController.StatusMessage);
            Assert.Equal("ChangePassword", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostChangePassword_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _manageController.ModelState.AddModelError("Test error", "Test error");

            var result = await _manageController.ChangePassword(new ChangePasswordViewModel());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task PostChangePassword_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.ChangePassword(new ChangePasswordViewModel()));

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostChangePassword_ReturnsViewResult_WhenCanNotChangePassword()
        {
            var model = new ChangePasswordViewModel { OldPassword = "1234", NewPassword = "abcDEF123_" };

            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.ChangePasswordAsync(It.IsAny<ApplicationUser>(), model.OldPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "01", Description = "Test error" }));

            var result = await _manageController.ChangePassword(model);
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ChangePasswordViewModel>(viewResult.Model);
            Assert.Single(_manageController.ModelState);
        }

        [Fact]
        public async Task SetPassword_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            var result = await _manageController.SetPassword();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<SetPasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task SetPassword_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.SetPassword());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task SetPassword_ReturnsRedirectToActionResult_WhenUserHasPassword()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.HasPasswordAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _manageController.SetPassword();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("ChangePassword", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostSetPassword_RedirectToActionResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.AddPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _signInManagerMock
                .Setup(manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            var result = await _manageController.SetPassword(new SetPasswordViewModel { NewPassword = "abcDEF123_" });
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Your password has been set.", _manageController.StatusMessage);
            Assert.Equal("SetPassword", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostSetPassword_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _manageController.ModelState.AddModelError("Test error", "Test error");

            var result = await _manageController.SetPassword(new SetPasswordViewModel { NewPassword = "abcDEF123_" });
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<SetPasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task PostSetPassword_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.SetPassword(new SetPasswordViewModel()));

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostSetPassword_ReturnsViewResult_WhenCanNotAddPassword()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.AddPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "01", Description = "Test error" }));

            var result = await _manageController.SetPassword(new SetPasswordViewModel { NewPassword = "abcDEF123_" });
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<SetPasswordViewModel>(viewResult.Model);
            Assert.Single(_manageController.ModelState);
        }

        [Fact]
        public async Task ExternalLogins_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.HasPasswordAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var result = await _manageController.ExternalLogins();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ExternalLoginsViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task ExternalLogins_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.ExternalLogins());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task LinkLoginCallback_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<ExternalLoginInfo>()))
                .ReturnsAsync(IdentityResult.Success);

            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExternalLoginInfo(new ClaimsPrincipal(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _manageController.LinkLoginCallback();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("The external login was added.", _manageController.StatusMessage);
            Assert.Equal("ExternalLogins", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task LinkLoginCallback_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.LinkLoginCallback());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task LinkLoginCallback_ThrowsApplicationException_WhenCanNotGetExternalLoginInfo()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1" });

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.LinkLoginCallback());

            Assert.Equal("Unexpected error occurred loading external login info for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task LinkLoginCallback_ThrowsApplicationException_WhenCanNotAddLogin()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1" });

            _userManagerMock
                .Setup(manager => manager.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<ExternalLoginInfo>()))
                .ReturnsAsync(IdentityResult.Failed());

            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExternalLoginInfo(new ClaimsPrincipal(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.LinkLoginCallback());

            Assert.Equal("Unexpected error occurred adding external login for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task RemoveLogin_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.RemoveLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _manageController.RemoveLogin(new RemoveLoginViewModel
            {
                LoginProvider = "Test provider",
                ProviderKey = "Test key"
            });
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("The external login was removed.", _manageController.StatusMessage);
            Assert.Equal("ExternalLogins", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task RemoveLogin_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.RemoveLogin(new RemoveLoginViewModel()));

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task RemoveLogin_ThrowsApplicationException_WhenCanNotRemoveLogin()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1" });

            _userManagerMock
                .Setup(manager => manager.RemoveLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.RemoveLogin(new RemoveLoginViewModel()));

            Assert.Equal("Unexpected error occurred removing external login for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task TwoFactorAuthentication_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            var result = await _manageController.TwoFactorAuthentication();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<TwoFactorAuthenticationViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task TwoFactorAuthentication_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.TwoFactorAuthentication());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task Disable2faWarning_ReturnsViewResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { TwoFactorEnabled = true });

            var result = await _manageController.Disable2faWarning();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Disable2fa", viewResult.ViewName);
        }

        [Fact]
        public async Task Disable2faWarning_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Disable2faWarning());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task Disable2faWarning_ThrowsApplicationException_WhenUserDoesNotHaveTwoFactorEnabled()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1", TwoFactorEnabled = false });

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Disable2faWarning());

            Assert.Equal("Unexpected error occured disabling 2FA for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task Disable2fa_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(manager => manager.SetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>(), false))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _manageController.Disable2fa();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("TwoFactorAuthentication", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Disable2fa_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Disable2fa());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task Disable2fa_ThrowsApplicationException_WhenCanNotSetTwoFactor()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1" });

            _userManagerMock
                .Setup(manager => manager.SetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>(), false))
                .ReturnsAsync(IdentityResult.Failed());

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.Disable2fa());

            Assert.Equal("Unexpected error occured disabling 2FA for user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task EnableAuthenticator_ReturnsViewResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(ManageUsersController => ManageUsersController.GetAuthenticatorKeyAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test_string");

            var result = await _manageController.EnableAuthenticator();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<EnableAuthenticatorViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task EnableAuthenticator_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.EnableAuthenticator());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostEnableAuthenticator_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(ManageUsersController => ManageUsersController.GetAuthenticatorKeyAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test_string");

            _userManagerMock
                .Setup(ManageUsersController =>
                    ManageUsersController.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(ManageUsersController =>
                    ManageUsersController.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<ApplicationUser>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            var result = await _manageController.EnableAuthenticator(new EnableAuthenticatorViewModel { Code = "Test code" });
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("ShowRecoveryCodes", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task PostEnableAuthenticator_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.EnableAuthenticator(new EnableAuthenticatorViewModel()));

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task PostEnableAuthenticator_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _manageController.ModelState.AddModelError("Test error", "Test error");

            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(ManageUsersController => ManageUsersController.GetAuthenticatorKeyAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test_string");

            _userManagerMock
                .Setup(ManageUsersController =>
                    ManageUsersController.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _manageController.EnableAuthenticator(new EnableAuthenticatorViewModel { Code = "test code" });
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<EnableAuthenticatorViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task PostEnableAuthenticator_ReturnsViewResult_WithErrors_WhenCanNotVerifyTwoFactorToken()
        {
            SetGetUserAsyncMethod();

            _userManagerMock
                .Setup(ManageUsersController => ManageUsersController.GetAuthenticatorKeyAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("test_string");

            _userManagerMock
                .Setup(ManageUsersController =>
                    ManageUsersController.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _manageController.EnableAuthenticator(new EnableAuthenticatorViewModel { Code = "test code" });
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<EnableAuthenticatorViewModel>(viewResult.Model);
            Assert.Single(_manageController.ModelState);
        }

        [Fact]
        public void ShowRecoveryCodes_ReturnsViewResult_WhenSucceeded()
        {
            string[] recoveryCodes = new string[] { "test code 1", "test code 2", "test code 3" };
            _manageController.TempData["RecoveryCodesKey"] = recoveryCodes;

            var result = _manageController.ShowRecoveryCodes();
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsAssignableFrom<ShowRecoveryCodesViewModel>(viewResult.Model);

            Assert.Equal(recoveryCodes, viewModel.RecoveryCodes);
        }

        [Fact]
        public void ShowRecoveryCodes_ReturnsRedirectToActionResult_WhenTempDataDoesNotHaveRecoveryCodes()
        {
            var result = _manageController.ShowRecoveryCodes();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("TwoFactorAuthentication", redirectToActionResult.ActionName);
        }

        [Fact]
        public void ResetAuthenticatorWarning_ReturnsViewResult_WhenSucceeded()
        {
            var result = _manageController.ResetAuthenticatorWarning();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("ResetAuthenticator", viewResult.ViewName);
        }

        [Fact]
        public async Task ResetAuthenticator_ReturnsRedirectToActionResult_WhenSucceded()
        {
            SetGetUserAsyncMethod();

            var result = await _manageController.ResetAuthenticator();
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("EnableAuthenticator", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task ResetAuthenticator_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.ResetAuthenticator());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task GenerateRecoveryCodesWarning_ReturnsViewResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { TwoFactorEnabled = true });

            var result = await _manageController.GenerateRecoveryCodesWarning();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("GenerateRecoveryCodes", viewResult.ViewName);
        }

        [Fact]
        public async Task GenerateRecoveryCodesWarning_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.GenerateRecoveryCodesWarning());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task GenerateRecoveryCodesWarning_ThrowsApplicationException_WhenUserDoesNotHaveTwoFactorEnabled()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { TwoFactorEnabled = false, Id = "1" });

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.GenerateRecoveryCodesWarning());

            Assert.Equal("Cannot generate recovery codes for user with ID '1' because they do not have 2FA enabled.", exception.Message);
        }

        [Fact]
        public async Task GenerateRecoveryCodes_ReturnsViewResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { TwoFactorEnabled = true });

            _userManagerMock
                .Setup(ManageUsersController =>
                    ManageUsersController.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<ApplicationUser>(), It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            var result = await _manageController.GenerateRecoveryCodes();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ShowRecoveryCodesViewModel>(viewResult.Model);
            Assert.Equal("ShowRecoveryCodes", viewResult.ViewName);
        }

        [Fact]
        public async Task GenerateRecoveryCodes_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.GenerateRecoveryCodes());

            Assert.Equal("Unable to load user with ID '1'.", exception.Message);
        }

        [Fact]
        public async Task GenerateRecoveryCodes_ThrowsApplicationException_WhenUserDoesNotHaveTwoFactorEnabled()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { TwoFactorEnabled = false, Id = "1" });

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _manageController.GenerateRecoveryCodes());

            Assert.Equal("Cannot generate recovery codes for user with ID '1' as they do not have 2FA enabled.", exception.Message);
        }

        [Fact]
        public async Task SendVerificationEmail_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "1", Email = "max@example.com" });

            var result = await _manageController.SendVerificationEmail(new IndexViewModel());

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Verification email sent. Please check your email.", _manageController.StatusMessage);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        private void SetGetUserAsyncMethod()
        {
            _userManagerMock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());
        }
    }
}
