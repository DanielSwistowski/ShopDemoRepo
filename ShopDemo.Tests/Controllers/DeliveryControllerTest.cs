using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.Controllers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class DeliveryControllerTest
    {
        Mock<IDeliveryService> mockDeliveryService;
        IMapper mapper;
        List<Delivery> deliveryOprions;
        Mock<ApplicationUserManager> mockUserManager;
        Mock<ControllerContext> mockControllerContext;

        [SetUp]
        public void SetUp()
        {
            mockDeliveryService = new Mock<IDeliveryService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DeliveryProfile());

            });
            mapper = config.CreateMapper();

            Mock<ICustomUserStore<ApplicationUser, int>> mockUserStore = new Mock<ICustomUserStore<ApplicationUser, int>>();
            mockUserManager = new Mock<ApplicationUserManager>(mockUserStore.Object);

            deliveryOprions = new List<Delivery>();
            deliveryOprions.Add(new Delivery { DeliveryId = 1, Option = "Odbiór osobisty", PaymentOption = PaymentOptions.NotApplicable, Price = 0, RealizationTimeInDays = 0, IsActive = true });
            deliveryOprions.Add(new Delivery { DeliveryId = 2, Option = "Przesyłka kurierska", PaymentOption = PaymentOptions.PaymentByTransfer, Price = 15, RealizationTimeInDays = 1, IsActive = true });
            deliveryOprions.Add(new Delivery { DeliveryId = 3, Option = "Przesyłka kurierska pobraniowa", PaymentOption = PaymentOptions.CashOnDelivery, Price = 20, RealizationTimeInDays = 1, IsActive = true });
            deliveryOprions.Add(new Delivery { DeliveryId = 4, Option = "Przesyłka", PaymentOption = PaymentOptions.CashOnDelivery, Price = 20, RealizationTimeInDays = 1, IsActive = false });

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
        public async Task SelectDeliveryOption_redirects_to_AddAddress_action_in_Manage_controller_if_user_address_is_not_provided()
        {
            mockUserManager.Setup(m => m.IsUserAddressSetAsync(It.IsAny<int>())).ReturnsAsync(false);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper, mockUserManager.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.SelectDeliveryOption() as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("AddAddress", result.RouteValues["action"]);
            Assert.AreEqual("Manage", result.RouteValues["controller"]);
        }

        [Test]
        public async Task SelectDeliveryOption_returns_correct_data()
        {
            mockDeliveryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Delivery, bool>>>())).ReturnsAsync(deliveryOprions.Where(d => d.IsActive == true).AsEnumerable());
            mockUserManager.Setup(m => m.IsUserAddressSetAsync(It.IsAny<int>())).ReturnsAsync(true);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper, mockUserManager.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.SelectDeliveryOption() as ViewResult;

            IEnumerable<DeliveryOptionsViewModel> deliveryOptionsModel = (IEnumerable<DeliveryOptionsViewModel>)result.Model;
            var deliveryOptionsArray = deliveryOptionsModel.ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, deliveryOptionsModel.Count());
            Assert.AreEqual(1, deliveryOptionsArray[0].DeliveryId);
            Assert.AreEqual("Odbiór osobisty", deliveryOptionsArray[0].Option);
            Assert.AreEqual(PaymentOptionsViewModel.NotApplicable, deliveryOptionsArray[0].PaymentOption);
            Assert.AreEqual(0, deliveryOptionsArray[0].Price);
            Assert.AreEqual(0, deliveryOptionsArray[0].RealizationTimeInDays);
        }
    }
}
