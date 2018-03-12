using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.Controllers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class PayuControllerTest
    {
        Mock<IPayuService> mockPayuService;
        Mock<IOrderService> mockOrderService;
        IMapper mapper;
        Mock<ControllerContext> mockControllerContext;

        [SetUp]
        public void SetUp()
        {
            mockPayuService = new Mock<IPayuService>();
            mockOrderService = new Mock<IOrderService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PayuProfile());
                cfg.AddProfile(new OrderProfile());
            });
            mapper = config.CreateMapper();

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Jan"));
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "1"));

            var genericIdentity = new GenericIdentity("");
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, new string[] { });

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);
            mockHttpRequest.Setup(m => m.UserHostAddress).Returns("127.0.0.1");

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(x => x.User).Returns(genericPrincipal);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
        }

        #region CreateNewOrder
        [Test]
        public async Task CreateNewOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CreateNewOrder() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CreateNewOrder_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CreateNewOrder(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task CreateNewOrder_creates_new_order_and_redirects_to_response_uri()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Order() { DeliveryOption = new Delivery { Option = "DeliveryOption", Price = 10 } });
            string redirectUri = "http://payu-payment.pl/";
            mockPayuService.Setup(m => m.CreateNewOrderAsync(It.IsAny<DataAccessLayer.Models.Payu.OrderCreateRequest>())).ReturnsAsync(new DataAccessLayer.Models.Payu.OrderCreateResponse() { RedirectUri = redirectUri });

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CreateNewOrder(10) as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(redirectUri, result.Url);
        }
        #endregion

        #region MakePaymentForAnExistingOrder
        [Test]
        public async Task MakePaymentForAnExistingOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.MakePaymentForAnExistingOrder() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task MakePaymentForAnExistingOrder_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.MakePaymentForAnExistingOrder(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task MakePaymentForAnExistingOrder_creates_new_order_and_redirects_to_response_uri()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Order() { DeliveryOption = new Delivery { Option = "DeliveryOption", Price = 10 } });
            string redirectUri = "http://payu-payment.pl/";
            mockPayuService.Setup(m => m.CreateNewOrderAsync(It.IsAny<DataAccessLayer.Models.Payu.OrderCreateRequest>())).ReturnsAsync(new DataAccessLayer.Models.Payu.OrderCreateResponse() { RedirectUri = redirectUri });
            mockOrderService.Setup(m => m.CreateNewOrderIdAsync(It.IsAny<Order>())).ReturnsAsync(It.IsAny<int>());

            Mock <UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.MakePaymentForAnExistingOrder(10) as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(redirectUri, result.Url);
            mockOrderService.Verify(m => m.CreateNewOrderIdAsync(It.IsAny<Order>()), Times.Once);
        }
        #endregion

        #region PaymentComplete
        [Test]
        public async Task PaymentComplete_returns_PaymentError_view_if_error_parameter_is_equal_to_501()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.PaymentComplete(It.IsAny<int>(), "501") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("PaymentError", result.ViewName);
        }

        [Test]
        public async Task PaymentComplete_changes_order_status()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            await controller.PaymentComplete(It.IsAny<int>());

            mockOrderService.Verify(m => m.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
        }
        #endregion
    }
}