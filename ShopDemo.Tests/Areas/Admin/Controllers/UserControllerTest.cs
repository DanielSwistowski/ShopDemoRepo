using AutoMapper;
using DataAccessLayer.Models;
using Microsoft.AspNet.Identity;
using Moq;
using NUnit.Framework;
using PagedList;
using Postal;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class UserControllerTest
    {
        Mock<ApplicationUserManager> mockUserManager;
        Mock<IEmailService> mockEmailService;
        IMapper mapper;

        Address address;
        ApplicationUser applicationUser;
        List<string> roles;

        [SetUp]
        public void SetUp()
        {
            Mock<ICustomUserStore<ApplicationUser, int>> mockUserStore = new Mock<ICustomUserStore<ApplicationUser, int>>();
            mockUserManager = new Mock<ApplicationUserManager>(mockUserStore.Object);

            mockEmailService = new Mock<IEmailService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());

            });
            mapper = config.CreateMapper();

            address = new Address { UserId = 1, City = "Warszawa", HouseNumber = "10a", Street = "Wiejska", ZipCode = "22-222" };
            applicationUser = new ApplicationUser { Id = 1, FirstName = "Jan", LastName = "Kowalski", UserName = "jankowalski@o2.pl" };

            roles = new List<string>();
            roles.Add("Admin");
            roles.Add("User");
        }

        [Test]
        public async Task Index_returns_paged_users_list()
        {
            List<ApplicationUser> usersList = new List<ApplicationUser>();
            usersList.Add(new ApplicationUser { Id = 1, FirstName = "FirstName1", LastName = "LastName1", Email = "email1@email.pl" });
            usersList.Add(new ApplicationUser { Id = 2, FirstName = "FirstName2", LastName = "LastName2", Email = "email2@email.pl" });
            usersList.Add(new ApplicationUser { Id = 3, FirstName = "FirstName3", LastName = "LastName3", Email = "email3@email.pl" });

            Tuple<IEnumerable<ApplicationUser>, int> usersTuple = Tuple.Create(usersList.AsEnumerable(), usersList.Count);

            mockUserManager.Setup(m => m.PageAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(usersTuple);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.Index(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) as ViewResult;
            var resultModel = (StaticPagedList<UsersListViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<StaticPagedList<UsersListViewModel>>(result.Model);
            Assert.AreEqual(3, resultModel.TotalItemCount);
            Assert.AreEqual(1, resultModel[0].Id);
            Assert.AreEqual("FirstName1", resultModel[0].FirstName);
            Assert.AreEqual("LastName1", resultModel[0].LastName);
            Assert.AreEqual("email1@email.pl", resultModel[0].Email);
        }

        #region SendMessage
        [Test]
        public void SendMessage_returns_BadRequest_if_email_is_not_provided()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = controller.SendMessage(It.IsAny<string>()) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void SendMessage_returns_view_with_model()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            string email = "test.email@email.com";

            var result = controller.SendMessage(email) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(email, ((SendEmailViewModel)result.Model).EmailTo);
        }

        [Test]
        public async Task SendMessage_returns_model_state_error_if_model_state_is_not_valid()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.SendMessage(It.IsAny<SendEmailViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task SendMessage_sent_email_and_redirects_to_Index_action()
        {
            mockEmailService.Setup(m => m.SendAsync(It.IsAny<Email>())).Returns(Task.FromResult(true));
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.SendMessage(new SendEmailViewModel() { Subject = "Test" }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<Email>()), Times.Once);
        }
        #endregion

        #region Details
        [Test]
        public async Task Details_returns_BadRequest_if_id_is_not_provided()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.Details() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Details_returns_NotFound_if_user_is_null()
        {
            ApplicationUser nullUser = null;
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullUser);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.Details(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_returns_correct_user_data()
        {
            mockUserManager.Setup(m => m.GetUserAddressAsync(It.IsAny<int>())).ReturnsAsync(address);
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<int>())).ReturnsAsync(roles);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.Details(10) as ViewResult;
            var resultModel = (UserDetailsVewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual("Jan", resultModel.FirstName);
            Assert.AreEqual("Kowalski", resultModel.LastName);
            Assert.AreEqual("Warszawa", resultModel.Address.City);
            Assert.AreEqual("10a", resultModel.Address.HouseNumber);
            CollectionAssert.AreEqual(roles, resultModel.Roles);
        }
        #endregion

        #region LockUserAccount
        [Test]
        public async Task LockUserAccount_returns_BadRequest_if_id_is_not_provided()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.LockUserAccount() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task LockUserAccount_returns_NotFound_if_user_is_null()
        {
            ApplicationUser nullUser = null;
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullUser);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.LockUserAccount(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task LockUserAccount_returns_view_with_model()
        {
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationUser());

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.LockUserAccount(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.That(((LockUserAccountViewModel)result.Model).Subject != string.Empty);
        }

        [Test]
        public async Task LockUserAccount_returns_model_state_error_if_model_state_is_not_valid()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.LockUserAccount(It.IsAny<LockUserAccountViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task LockUserAccount_returns_model_state_error_if_user_try_lock_out_his_own_account()
        {
            string loggedUser = "jankowaski@o2.pl";
            Mock<HttpContextBase> mockContextBase = new Mock<HttpContextBase>();
            var fakeIdentity = new GenericIdentity(loggedUser);
            var fakePrincipal = new GenericPrincipal(fakeIdentity, null);
            mockContextBase.Setup(m => m.User).Returns(fakePrincipal);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(p => p.HttpContext).Returns(mockContextBase.Object);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.LockUserAccount(new LockUserAccountViewModel { Email = loggedUser }) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
            Assert.AreEqual("Nie możesz zablokować konta na które jesteś zalogowany!", result.ViewData.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task LockUserAccount_lock_user_account_and_sent_email_and_redirects_to_Details_action_with_id_param()
        {
            string loggedUser = "admin@test.com";
            Mock<HttpContextBase> mockContextBase = new Mock<HttpContextBase>();
            var fakeIdentity = new GenericIdentity(loggedUser);
            var fakePrincipal = new GenericPrincipal(fakeIdentity, null);
            mockContextBase.Setup(m => m.User).Returns(fakePrincipal);

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(p => p.HttpContext).Returns(mockContextBase.Object);

            mockEmailService.Setup(m => m.SendAsync(It.IsAny<Email>())).Returns(Task.FromResult(true));
            mockUserManager.Setup(m => m.LockUserAccountAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.LockUserAccount(new LockUserAccountViewModel { Id = 10, Email= "jankowaski@o2.pl", Subject = "Test" }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["id"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<Email>()), Times.Once);
            mockUserManager.Verify(m => m.LockUserAccountAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region LockUserAccount
        [Test]
        public async Task UnlockUserAccount_returns_BadRequest_if_id_is_not_provided()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.UnlockUserAccount() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UnlockUserAccount_returns_NotFound_if_user_is_null()
        {
            ApplicationUser nullUser = null;
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullUser);

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.UnlockUserAccount(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UnlockUserAccount_returns_view_with_model()
        {
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ApplicationUser());

            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.UnlockUserAccount(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.That(((UnlockUserAccountViewModel)result.Model).Subject != string.Empty);
        }

        [Test]
        public async Task UnlockUserAccount_returns_model_state_error_if_model_state_is_not_valid()
        {
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.UnlockUserAccount(It.IsAny<UnlockUserAccountViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task UnlockUserAccount_unlock_user_account_and_sent_email_and_redirects_to_Details_action_with_id_param()
        {
            mockEmailService.Setup(m => m.SendAsync(It.IsAny<Email>())).Returns(Task.FromResult(true));
            mockUserManager.Setup(m => m.UnlockUserAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(true));
            UserController controller = new UserController(mockUserManager.Object, mapper, mockEmailService.Object);

            var result = await controller.UnlockUserAccount(new UnlockUserAccountViewModel { Id = 10, Subject="Test" }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["id"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<Email>()), Times.Once);
            mockUserManager.Verify(m => m.UnlockUserAccountAsync(It.IsAny<int>()), Times.Once);
        }
        #endregion
    }
}
