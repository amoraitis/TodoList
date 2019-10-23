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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TodoList.Core.Models;
using TodoList.UnitTests.Resources;
using TodoList.Web.Controllers;
using TodoList.Web.Models.AccountViewModels;
using Xunit;

namespace TodoList.UnitTests.Controllers
{
    public class AccountControllerTest
    {
        private readonly Mock<FakeUserManager> _userManagerMock;
        private readonly Mock<FakeSignInManager> _signInManagerMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;

        private readonly AccountController _accountController;

        public AccountControllerTest()
        {
            _userManagerMock = new Mock<FakeUserManager>();
            _signInManagerMock = new Mock<FakeSignInManager>();
            _emailSenderMock = new Mock<IEmailSender>();
            _loggerMock = new Mock<ILogger<AccountController>>();

            _signInManagerMock
                .Setup(manager => manager.GetTwoFactorAuthenticationUserAsync())
                .ReturnsAsync(new ApplicationUser());

            _accountController = new AccountController(_userManagerMock.Object,
                    _signInManagerMock.Object,
                    _emailSenderMock.Object,
                    _loggerMock.Object);

            _accountController.ControllerContext = new ControllerContext();
            _accountController.ControllerContext.HttpContext = new DefaultHttpContext();

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

            _accountController.ControllerContext.HttpContext.RequestServices = servicesMock.Object;
            _accountController.Url = Mock.Of<IUrlHelper>();
        }

        [Fact]
        public async Task GetLogin_ReturnsViewResult()
        {
            var result = await _accountController.Login();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async Task PostLogin_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            var user = new LoginViewModel { Email = "max@example", Password = "abcDEF123_", RememberMe = true };

            _signInManagerMock
                .Setup(manager => manager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _accountController.Login(user);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLogin_ReturnsViewResult_WhenIsModelIsNotValid()
        {
            _accountController.ModelState.AddModelError("Test error", "Test error");

            var result = await _accountController.Login(new LoginViewModel());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<LoginViewModel>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task PostLogin_ReturnsRedirectToActionResult_WhenIsIsLockedOut()
        {
            var user = new LoginViewModel { Email = "max@example", Password = "abcDEF123_", RememberMe = true };

            _signInManagerMock
                .Setup(manager => manager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var result = await _accountController.Login(user);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Lockout", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLogin_ReturnsRedirectToActionResult_WhenRequiredTwoAuthenticationFactors()
        {
            var user = new LoginViewModel { Email = "max@example", Password = "abcDEF123_", RememberMe = true };

            _signInManagerMock
                .Setup(manager => manager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

            var result = await _accountController.Login(user);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LoginWith2fa", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLogin_ReturnsViewResult_WhenLoginAttemptIsInvalid()
        {
            var user = new LoginViewModel { Email = "max@example", Password = "abcDEF123_", RememberMe = true };

            _signInManagerMock
                .Setup(manager => manager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false))
                .ReturnsAsync(new Microsoft.AspNetCore.Identity.SignInResult());

            var result = await _accountController.Login(user);
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<LoginViewModel>(viewResult.ViewData.Model);
            Assert.Equal("Invalid login attempt.", _accountController.ModelState["INVALID_LOGIN_ATTEMPT"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task LoginWith2fa_ReturnsViewResult_WhenSucceeded()
        {
            var result = await _accountController.LoginWith2fa(true);
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<LoginWith2faViewModel>(viewResult.ViewData.Model);
            Assert.Null(viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async Task LoginWith2fa_ThrowsApplicationException_WhenUserIsNull()
        {
            SetGetTwoFactorAuthenticationUserAsyncToNull();

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _accountController.LoginWith2fa(true));

            Assert.Equal("Unable to load two-factor authentication user.", exception.Message);
        }

        private void SetGetTwoFactorAuthenticationUserAsyncToNull()
        {
            _signInManagerMock
                .Setup(manager => manager.GetTwoFactorAuthenticationUserAsync())
                .ReturnsAsync(() => null);
        }

        [Fact]
        public async Task PostLoginWith2fa_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _accountController.LoginWith2fa(new LoginWith2faViewModel { TwoFactorCode = "123 456" }, true, null);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLoginWith2fa_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _accountController.ModelState.AddModelError("Test error", "Test error");

            var result = await _accountController.LoginWith2fa(new LoginWith2faViewModel { TwoFactorCode = "123 456" }, true, null);

            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task PostLoginWith2fa_ThrowsApplicationException_WhenUserIsNull()
        {
            SetGetTwoFactorAuthenticationUserAsyncToNull();

            _userManagerMock
                .Setup(manager => manager.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("1234");

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _accountController.LoginWith2fa(new LoginWith2faViewModel { TwoFactorCode = "123 456" }, true, null));

            Assert.Equal("Unable to load user with ID '1234'.", exception.Message);
        }

        [Fact]
        public async Task PostLoginWith2fa_ReturnsRedirectToActionResult_WhenIsLockedOut()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var result = await _accountController.LoginWith2fa(new LoginWith2faViewModel { TwoFactorCode = "123 456" }, true, null);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Lockout", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLoginWith2fa_ReturnsViewResult_WithErrors_WhenIsFailed()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _accountController.LoginWith2fa(new LoginWith2faViewModel { TwoFactorCode = "123 456" }, true, null);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Equal("Invalid authenticator code.", _accountController.ModelState["INVALID_AUTHENTICATOR_CODE"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task LoginWithRecoveryCode_ReturnsViewResult_WhenSucceeded()
        {
            var result = await _accountController.LoginWithRecoveryCode();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async Task LoginWithRecoveryCode_ThrowsApplicationException_WhenUserIsNull()
        {
            SetGetTwoFactorAuthenticationUserAsyncToNull();

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _accountController.LoginWithRecoveryCode());

            Assert.Equal("Unable to load two-factor authentication user.", exception.Message);
        }

        [Fact]
        public async Task PostLoginWithRecoveryCode_ReturnsRedictResult_WhenSucceeded()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _accountController.LoginWithRecoveryCode(
                new LoginWithRecoveryCodeViewModel { RecoveryCode = "123 456" });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLoginWithRecoveryCode_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _accountController.ModelState.AddModelError("Test error", "Test error");

            var result = await _accountController.LoginWithRecoveryCode(
                new LoginWithRecoveryCodeViewModel { RecoveryCode = "123 456" });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.IsAssignableFrom<LoginWithRecoveryCodeViewModel>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task PostLoginWithRecoveryCode_ThrowsApplicationException_WhenUserIsNull()
        {
            SetGetTwoFactorAuthenticationUserAsyncToNull();

            var exception = await Assert.ThrowsAsync<ApplicationException>(async () =>
               await _accountController.LoginWithRecoveryCode(
                   new LoginWithRecoveryCodeViewModel { RecoveryCode = "123 456" }));

            Assert.Equal("Unable to load two-factor authentication user.", exception.Message);
        }

        [Fact]
        public async Task PostLoginWithRecoveryCode_ReturnsRedirectResult_WhenIsLockedOut()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var result = await _accountController.LoginWithRecoveryCode(
                new LoginWithRecoveryCodeViewModel { RecoveryCode = "123 456" });

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Lockout", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task PostLoginWithRecoveryCode_ReturnsViewResult_WhenRecoveryCodeIsInvalid()
        {
            _signInManagerMock
                .Setup(manager => manager.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _accountController.LoginWithRecoveryCode(
                new LoginWithRecoveryCodeViewModel { RecoveryCode = "123 456" });
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Equal("Invalid recovery code entered.", _accountController.ModelState["INVALID_RECOVERY_CODE"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Lockout_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.Lockout();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Register_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.Register();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Logout_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            var result = await _accountController.Logout();

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(new ClaimsPrincipal(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _signInManagerMock
                .Setup(manager => manager.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _accountController.ExternalLoginCallback();

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsRedirectToActionResult_WithErrorMessage_WhenRemoteError()
        {
            var result = await _accountController.ExternalLoginCallback(null, "Test error");

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsRedirectToActionResult_WhenCanNotGetExternalLoginInfo()
        {
            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(() => null);

            var result = await _accountController.ExternalLoginCallback();

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsRedirectToActionResult_WhenIsLockedOut()
        {
            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(new ClaimsPrincipal(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _signInManagerMock
                .Setup(manager => manager.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var result = await _accountController.ExternalLoginCallback();

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Lockout", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ExternalLoginCallback_ReturnsViewResult_WhenFailed()
        {
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal();

            identity.AddClaim(new Claim(ClaimTypes.Email, "max@example.com"));
            claimsPrincipal.AddIdentity(identity);

            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(claimsPrincipal, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _signInManagerMock
                .Setup(manager => manager.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _accountController.ExternalLoginCallback();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ExternalLoginViewModel>(viewResult.ViewData.Model);

            Assert.Equal("ExternalLogin", viewResult.ViewName);
            Assert.Equal("max@example.com", model.Email);
        }

        [Fact]
        public async Task ExternalLoginConfirmation_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(new ClaimsPrincipal(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _userManagerMock
                .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            _userManagerMock
                .Setup(manager => manager.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<ExternalLoginInfo>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            var result = await _accountController.ExternalLoginConfirmation(new ExternalLoginViewModel { Email = "max@example.com" });

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ExternalLoginConfirmation_ReturnsViewResult_WhenModelStateIsNotValid()
        {
            _accountController.ModelState.AddModelError("Test error", "Test error");

            var result = await _accountController.ExternalLoginConfirmation(new ExternalLoginViewModel { Email = "max@example.com" });

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ExternalLoginViewModel>(viewResult.ViewData.Model);
            Assert.Equal("ExternalLogin", viewResult.ViewName);
        }

        [Fact]
        public async Task ExternalLoginConfirmation_ThrowsApplicationException_WhenCanNotGetExternalLoginInfo()
        {
            _signInManagerMock
                .Setup(manager => manager.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(() => null);

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _accountController.ExternalLoginConfirmation(new ExternalLoginViewModel { Email = "max@example.com" }));

            Assert.Equal("Error loading external login information during confirmation.", exception.Message);
        }

        [Fact]
        public async Task ConfirmEmail_ReturnsViewResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser());

            _userManagerMock
                .Setup(manager => manager.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            var result = await _accountController.ConfirmEmail("1234", "test_code");
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("ConfirmEmail", viewResult.ViewName);
        }

        [Fact]
        public async Task ConfirmEmail_ReturnsRedirectToActionResult_WhenUserIdOrCodeIsNull()
        {
            var result = await _accountController.ConfirmEmail(null, null);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public async Task ConfirmEmail_ThrowsApplicationException_WhenCanNotFindUser()
        {
            _userManagerMock
                .Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _accountController.ConfirmEmail("1234", "test_code"));

            Assert.Equal("Unable to load user with ID '1234'.", exception.Message);
        }

        [Fact]
        public async Task ConfirmEmail_ReturnsViewResult_WhenFailed()
        {
            _userManagerMock
                .Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser());

            _userManagerMock
                .Setup(manager => manager.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Failed());

            var result = await _accountController.ConfirmEmail("1234", "test_code");
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Error", viewResult.ViewName);
        }

        [Fact]
        public void ForgotPassword_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.ForgotPassword();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ForgotPasswordConfirmation_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.ForgotPasswordConfirmation();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ResetPassword_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.ResetPassword("test_code");
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ResetPasswordViewModel>(viewResult.Model);

            Assert.Equal("test_code", model.Code);
        }

        [Fact]
        public async Task PostResetPassword_ReturnsRedirectToActionResult_WhenSucceeded()
        {
            _userManagerMock
                .Setup(manager => manager.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser());

            _userManagerMock
                .Setup(manager => manager.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _accountController.ResetPassword(
                new ResetPasswordViewModel { Email = "max@example.com", Code = "test_code", Password = "abcDEF123_" });
            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("ResetPasswordConfirmation", actionResult.ActionName);
        }

        [Fact]
        public async Task PostResetPassword_ReturnsRedirectToActionResult_WhenModelStateIsNotValid()
        {
            _accountController.ModelState.AddModelError("Test error", "Test error");

            var result = await _accountController.ResetPassword(new ResetPasswordViewModel());
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<ResetPasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task PostResetPassword_ReturnsRedirectToActionResult_WhenCanNotFindUserByEmail()
        {
            _userManagerMock
                .Setup(manager => manager.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var result = await _accountController.ResetPassword(
                new ResetPasswordViewModel { Email = "max@example.com" });
            var actionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("ResetPasswordConfirmation", actionResult.ActionName);
        }

        [Fact]
        public async Task PostResetPassword_ReturnsViewResult_WithErrors_WhenIsFailed()
        {
            _userManagerMock
                .Setup(manager => manager.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser());

            _userManagerMock
                .Setup(manager => manager.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "01", Description = "Test error" }));

            var result = await _accountController.ResetPassword(
                new ResetPasswordViewModel { Email = "max@example.com", Code = "test_code", Password = "abcDEF123_" });

            Assert.IsType<ViewResult>(result);
            Assert.False(_accountController.ModelState.IsValid);
            Assert.Single(_accountController.ModelState);
        }

        [Fact]
        public void ResetPasswordConfirmation_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.ResetPasswordConfirmation();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AccessDenied_ReturnsViewResult_WhenSucceeded()
        {
            var result = _accountController.AccessDenied();

            Assert.IsType<ViewResult>(result);
        }
    }
}
