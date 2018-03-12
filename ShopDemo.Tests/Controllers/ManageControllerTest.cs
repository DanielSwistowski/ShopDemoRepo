using AutoMapper;
using DataAccessLayer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.Controllers;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System;
using System.Security.Claims;
using ShopDemo.ViewModels;
using static ShopDemo.Controllers.ManageController;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class ManageControllerTest
    {
        Mock<ApplicationSignInManager> mockSigInManager;
        Mock<ApplicationUserManager> mockUserManager;
        IMapper mapper;

        Address address;
        ApplicationUser applicationUser;
        List<string> roles;

        Mock<ControllerContext> mockControllerContext;

        [SetUp]
        public void SetUp()
        {
            Mock<ICustomUserStore<ApplicationUser, int>> mockUserStore = new Mock<ICustomUserStore<ApplicationUser, int>>();
            mockUserManager = new Mock<ApplicationUserManager>(mockUserStore.Object);

            Mock<IAuthenticationManager> mockAuthenticationManager = new Mock<IAuthenticationManager>();
            mockSigInManager = new Mock<ApplicationSignInManager>(mockUserManager.Object, mockAuthenticationManager.Object);

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
            
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Jan"));
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "1"));

            var genericIdentity = new GenericIdentity("");
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, new string[] { });

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.SetupGet(x => x.User).Returns(genericPrincipal);
            mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
        }

        [Test]
        public async Task Index_returns_correct_data()
        {
            mockUserManager.Setup(m => m.GetUserAddressAsync(It.IsAny<int>())).ReturnsAsync(address);
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<int>())).ReturnsAsync(roles);

            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Index(null) as ViewResult;
            var resultModel = (IndexViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual("Jan", resultModel.PersonalData.FirstName);
            Assert.AreEqual("Kowalski", resultModel.PersonalData.LastName);
            Assert.AreEqual("Warszawa", resultModel.Address.City);
            Assert.AreEqual("10a", resultModel.Address.HouseNumber);
            CollectionAssert.AreEqual(roles, resultModel.PersonalData.Roles);
        }

        [Test]
        public async Task EditPersonalData_returns_model_state_error_if_model_state_is_not_valid()
        {
            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.EditPersonalData(new EditPersonalDataViewModel()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EditPersonalData_returns_model_state_error_if_UpdateAsync_result_is_not_succeeded()
        {
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Failed("Error"));

            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;
            controller.ModelState.Clear();

            var result = await controller.EditPersonalData(new EditPersonalDataViewModel()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EditPersonalData_redirects_to_Index_action_if_UpdateAync_result_is_succeeded()
        {
            mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(applicationUser);
            mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.EditPersonalData(new EditPersonalDataViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(ManageMessageId.ChangePersonalDataSuccess, result.RouteValues["Message"]);
        }

        [Test]
        public async Task EditAddress_returns_model_state_error_if_model_state_is_not_valid()
        {
            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.EditAddress(new EditAddressViewModel()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EditAddress_redirects_to_Index_action_if_UpdateUserAddressAsync_ended_successfull()
        {
            mockUserManager.Setup(m => m.UpdateUserAddressAsync(new Address()));

            ManageController controller = new ManageController(mockUserManager.Object, mockSigInManager.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.EditAddress(new EditAddressViewModel()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(ManageMessageId.ChangeAddressSuccess, result.RouteValues["Message"]);
        }
    }
}