using Moq;
using NUnit.Framework;
using Postal;
using Service_Layer.Services;
using ShopDemo.Controllers;
using System;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using ShopDemo.ViewModels;
using System.Web.Mvc;
using DataAccessLayer.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using Microsoft.AspNet.Identity;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTest
    {
        Mock<ApplicationSignInManager> mockSignInManager;
        Mock<ApplicationUserManager> mockUserManager;
        Mock<IAuthenticationManager> mockAuthenticationManager;
        Mock<IEmailService> mockEmailService;
        Mock<ICustomUserStore<ApplicationUser, int>> mockUserStore;

        [SetUp]
        public void SetUp()
        {
            mockAuthenticationManager = new Mock<IAuthenticationManager>();
            mockEmailService = new Mock<IEmailService>();
            mockUserStore = new Mock<ICustomUserStore<ApplicationUser, int>>();
            mockUserManager = new Mock<ApplicationUserManager>(mockUserStore.Object);
            mockSignInManager = new Mock<ApplicationSignInManager>(mockUserManager.Object, mockAuthenticationManager.Object);
        }


        #region Login
        [Test]
        public async Task Login_returns_model_state_error_if_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.Login(It.IsAny<LoginViewModel>(), It.IsAny<string>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Login_returns_model_state_error_if_user_is_not_found()
        {
            ApplicationUser applicationUser = null;
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();

            var result = await controller.Login(new LoginViewModel(), It.IsAny<string>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Login_redirects_to_AccountIsLocked_action_if_user_account_is_disabled()
        {
            ApplicationUser applicationUser = new ApplicationUser { AccountIsEnabled = false };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.Login(new LoginViewModel(), It.IsAny<string>()) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("AccountIsLocked", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
        }

        [Test]
        public async Task Login_redirects_to_EmailIsNotConfirm_action_if_user_email_is_not_confirmed()
        {
            ApplicationUser applicationUser = new ApplicationUser { AccountIsEnabled = true };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(false);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.Login(new LoginViewModel(), It.IsAny<string>()) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("EmailIsNotConfirm", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
        }

        [Test]
        public async Task Login_redirects_to_return_url_if_login_ended_successfully()
        {
            ApplicationUser applicationUser = new ApplicationUser { AccountIsEnabled = true };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(true);
            string returnUrl = "/shopdemo/produkty";

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.IsLocalUrl(It.IsAny<string>())).Returns(true);

            mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInStatus.Success);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;

            var result = await controller.Login(new LoginViewModel(), returnUrl) as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
        }

        [Test]
        public async Task Login_returns_Lockout_view_if_it_was_to_many_login_attempts()
        {
            ApplicationUser applicationUser = new ApplicationUser { AccountIsEnabled = true };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(true);

            mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInStatus.LockedOut);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.Login(new LoginViewModel(), It.IsAny<string>()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Lockout", result.ViewName);
        }

        [Test]
        public async Task Login_returns_model_state_error_if_sign_in_status_is_failure()
        {
            ApplicationUser applicationUser = new ApplicationUser { AccountIsEnabled = true };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(true);

            mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInStatus.Failure);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();

            var result = await controller.Login(new LoginViewModel(), It.IsAny<string>()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }
        #endregion

        #region AccountIsLocked
        [Test]
        public async Task AccountIsLocked_returns_model_state_error_if_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.AccountIsLocked(It.IsAny<AccountIsLockedViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task AccountIsLocked_redirects_to_Login_action_if_user_is_null()
        {
            ApplicationUser applicationUser = null;
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.AccountIsLocked(new AccountIsLockedViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
        }

        [Test]
        public async Task AccountIsLocked_sent_email_and_redirects_to_MessageWasSent_action()
        {
            ApplicationUser applicationUser = new ApplicationUser { LockAccountReason = new LockAccountReason() };
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);
            mockEmailService.Setup(m => m.SendAsync(new AdminMessageEmail()));

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.AccountIsLocked(new AccountIsLockedViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("MessageWasSent", result.RouteValues["action"]);
            Assert.IsNull(result.RouteValues["controller"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<AdminMessageEmail>()), Times.Once);
        }
        #endregion

        #region EmailIsNotConfirm
        [Test]
        public async Task EmailIsNotConfirm_returns_model_state_error_is_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.EmailIsNotConfirm(It.IsAny<RestoreActivationLinkViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EmailIsNotConfirm_returns_NotFound_if_user_is_null()
        {
            ApplicationUser applicationUser = null;
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.EmailIsNotConfirm(new RestoreActivationLinkViewModel()) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task EmailIsNotConfirm_sent_email_and_redirects_to_ConfirmEmailInfo_action()
        {
            mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            string emailConfirmationToken = "token";
            mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<int>())).ReturnsAsync(emailConfirmationToken);
            mockEmailService.Setup(m => m.SendAsync(new AccountManagementEmail()));

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = controllerContext;

            var result = await controller.EmailIsNotConfirm(new RestoreActivationLinkViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ConfirmEmailInfo", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<AccountManagementEmail>()), Times.Once);
        }
        #endregion

        #region Register
        [Test]
        public async Task Register_returns_model_state_error_if_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.Register(It.IsAny<RegisterViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Register_returns_model_state_error_if_CreateAsync_identity_result_is_not_succeeded()
        {
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("Error"));

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();

            var result = await controller.Register(new RegisterViewModel()) as ViewResult;
            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Register_addds_user_role_and_sent_confirmation_email_and_redirects_to_ConfirmAccountInfo_if_CreateAsync_identity_result_is_succeeded()
        {
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(m => m.AddToRoleAsync(10, It.IsAny<string>()));
            mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<int>())).ReturnsAsync(It.IsAny<string>());
            mockEmailService.Setup(m => m.SendAsync(new AccountManagementEmail()));

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = controllerContext;

            var result = await controller.Register(new RegisterViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ConfirmAccountInfo", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<AccountManagementEmail>()), Times.Once);
        }
        #endregion

        #region ConfirmEmail
        [Test]
        public async Task ConfirmEmail_returns_Error_view_if_userId_is_default_int()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.ConfirmEmail(default(int), "code") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task ConfirmEmail_returns_Error_view_if_code_is_null()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.ConfirmEmail(10, null) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task ConfirmEmail_returns_Error_view_if_ConfirmEmailAsync_result_is_not_Succedded()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            mockUserManager.Setup(m => m.ConfirmEmailAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

            var result = await controller.ConfirmEmail(10, "code") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task ConfirmEmail_returns_ConfirmEmail_view_if_ConfirmEmailAsync_result_is_Succedded()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            mockUserManager.Setup(m => m.ConfirmEmailAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var result = await controller.ConfirmEmail(10, "code") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ConfirmEmail", result.ViewName);
        }
        #endregion

        #region ForgotPassword
        [Test]
        public async Task ForgotPassword_returns_model_state_error_if_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.ForgotPassword(It.IsAny<ForgotPasswordViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task ForgotPassword_returns_ForgotPasswordConfirmation_view_if_user_is_null()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            ApplicationUser applicationUser = null;
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            var result = await controller.ForgotPassword(new ForgotPasswordViewModel()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ForgotPasswordConfirmation", result.ViewName);
        }

        [Test]
        public async Task ForgotPassword_returns_ForgotPasswordConfirmation_view_if_user_email_is_not_confirmed()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await controller.ForgotPassword(new ForgotPasswordViewModel()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ForgotPasswordConfirmation", result.ViewName);
        }

        [Test]
        public async Task ForgotPassword_sent_email_and_redirects_to_ForgotPasswordConfirmation_action()
        {
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            mockUserManager.Setup(m => m.IsEmailConfirmedAsync(It.IsAny<int>())).ReturnsAsync(true);
            mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<int>())).ReturnsAsync(It.IsAny<string>());

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = controllerContext;

            var result = await controller.ForgotPassword(new ForgotPasswordViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ForgotPasswordConfirmation", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<AccountManagementEmail>()), Times.Once);
        }
        #endregion

        #region ResetPassword
        [Test]
        public void ResetPassword_returns_Error_view_if_code_is_null()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            string code = null;
            var result = controller.ResetPassword(code) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public void ResetPassword_returns_default_view_if_code_is_not_null()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            string code = "code";
            var result = controller.ResetPassword(code) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.ViewName);
        }

        [Test]
        public async Task ResetPassword_returns_model_state_error_if_model_state_is_not_valid()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.ResetPassword(It.IsAny<ResetPasswordViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task ResetPassword_redirects_to_ResetPasswordConfirmation_action_if_user_is_null()
        {
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            ApplicationUser applicationUser = null;
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(applicationUser);

            var result = await controller.ResetPassword(new ResetPasswordViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ResetPasswordConfirmation", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
        }

        [Test]
        public async Task ResetPassword_returns_model_state_error_if_ResetPasswordAsync_result_is_not_succeeded()
        {
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("Error"));

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ModelState.Clear();

            var result = await controller.ResetPassword(new ResetPasswordViewModel()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task ResetPassword_redirects_to_ResetPasswordConfirmation_action_if_ResetPasswordAsync_result_is_succeeded()
        {
            mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());
            mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);

            var result = await controller.ResetPassword(new ResetPasswordViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ResetPasswordConfirmation", result.RouteValues["action"]);
            Assert.AreEqual("Account", result.RouteValues["controller"]);
        }
        #endregion

        [Test]
        public void LogOff_clear_session_and_redirects_to_Index_action_in_Home_controller()
        {
            mockAuthenticationManager.Setup(m => m.SignOut(It.IsAny<string>()));

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            Mock<HttpSessionStateBase> mockSession = new Mock<HttpSessionStateBase>();
            mockSession.Setup(m => m.Abandon());
            mockHttpContext.Setup(m => m.Session).Returns(mockSession.Object);
            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);
            
            AccountController controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockAuthenticationManager.Object, mockEmailService.Object);
            controller.ControllerContext = controllerContext;

            var result = controller.LogOff() as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Home", result.RouteValues["controller"]);
            mockSession.Verify(m => m.Abandon(),Times.Once);
        }
    }
}