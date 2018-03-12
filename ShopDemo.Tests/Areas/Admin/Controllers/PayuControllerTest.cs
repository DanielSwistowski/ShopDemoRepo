using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.AutoMapperProfiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
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
                cfg.AddProfile(new OrderProfile());
            });
            mapper = config.CreateMapper();

            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(m => m.InputStream).Returns(new MemoryStream());
            mockRequest.SetupGet(x => x.Headers).Returns(new WebHeaderCollection { { "OpenPayu-Signature", "TestSignature" } });

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

            mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockContext.Object);
        }

        #region PayuPaymentStatusNotify
        [Test]
        public async Task PayuPaymentStatusNotify_returns_InternalServerError_if_payu_request_signature_is_not_valid()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(It.IsAny<DataAccessLayer.Models.Payu.Order>()) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task PayuPaymentStatusNotify_returns_InternalServerError_if_resonse_from_payu_not_contains_ExtOrderId()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(new DataAccessLayer.Models.Payu.Order { OrderId = "POIU0987", Status = "REJECTED" }) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task PayuPaymentStatusNotify_returns_OK_status_code_if_existing_PayuOrderId_is_equal_to_OrderId_and_PaymentStatus_is_COMPLETED()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Order existingOrder = new Order() { OrderId = 10, PayuData = new PayuOrderData { PayuOrderId = "POIU0987", PaymentStatus = "COMPLETED" } };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(existingOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(new DataAccessLayer.Models.Payu.Order { OrderId = "POIU0987", ExtOrderId = "10" }) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task PayuPaymentStatusNotify_save_payment_and_returns_OK_status_code_if_existing_PayuOrderId_is_equal_to_OrderId_and_PaymentStatus_is_CANCELED_and_new_order_status_is_REJECTED()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Order existingOrder = new Order() { OrderId = 10, PayuData = new PayuOrderData { PayuOrderId = "POIU0987", PaymentStatus = "CANCELED" } };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(existingOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(new DataAccessLayer.Models.Payu.Order { OrderId = "POIU0987", ExtOrderId = "10", Status = "REJECTED" }) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            mockOrderService.Verify(m => m.SavePaymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task PayuPaymentStatusNotify_save_payment_and_returns_OK_status_code_if_order_status_is_COMPLETED()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Order existingOrder = new Order() { OrderId = 10, PayuData = null };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(existingOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(new DataAccessLayer.Models.Payu.Order { ExtOrderId = "10", Status = "COMPLETED" }) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            mockOrderService.Verify(m => m.SavePaymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task PayuPaymentStatusNotify_save_payment_and_returns_OK_status_code_if_order_status_is_CANCELED()
        {
            mockPayuService.Setup(m => m.PayuSignatureIsValid(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Order existingOrder = new Order() { OrderId = 10, PayuData = null };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(existingOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.PayuPaymentStatusNotify(new DataAccessLayer.Models.Payu.Order { ExtOrderId = "10", Status = "CANCELED" }) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            mockOrderService.Verify(m => m.SavePaymentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        #region Refund
        [Test]
        public async Task Refund_returns_BadRequest_if_orderId_is_not_provided()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.Refund() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Refund_sets_order_status_to_Uncompleted_and_redirects_to_Details_action_in_Order_controller_if_refund_order_response_statusCode_is_not_SUCCESS()
        {
            mockPayuService.Setup(m => m.RefundAsync(It.IsAny<string>())).ReturnsAsync(new DataAccessLayer.Models.Payu.RefundOrderResponse { status = new DataAccessLayer.Models.Payu.Status { statusCode = "Error" } });

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.Refund(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.ChangeStatusAsync(10, OrderStatus.Uncompleted), Times.Once);
        }

        [Test]
        public async Task Refund_redirects_to_CancelOrderSuccess_action_in_Order_controller_if_refund_order_response_statusCode_is_SUCCESS()
        {
            mockPayuService.Setup(m => m.RefundAsync(It.IsAny<string>())).ReturnsAsync(new DataAccessLayer.Models.Payu.RefundOrderResponse { status = new DataAccessLayer.Models.Payu.Status { statusCode = "SUCCESS" } });

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.Refund(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("CancelOrderSuccess", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.ChangeStatusAsync(10, OrderStatus.Uncompleted), Times.Never);
        }

        [Test]
        public async Task Refund_sets_order_status_to_Uncompleted_and_redirects_to_Details_action_in_Order_controller_if_refund_order_response_throws_exception()
        {
            mockPayuService.Setup(m => m.RefundAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.Refund(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.ChangeStatusAsync(10, OrderStatus.Uncompleted), Times.Once);
        }
        #endregion

        #region AcceptPayment
        [Test]
        public async Task AcceptPayment_returns_BadRequest_if_orderId_is_not_provided()
        {
            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.AcceptPayment() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task AcceptPayment_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.AcceptPayment(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task AcceptPayment_redirects_to_Details_action_in_Order_controller_if_order_status_is_not_equal_to_PaymentRejected()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Order { OrderStatus = OrderStatus.WaitingForPayment });

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.AcceptPayment(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
        }
        #endregion

        #region AcceptPaymentConfirm
        [Test]
        public async Task AcceptPaymentConfirm_redirects_to_Details_action_in_Order_controller_if_accept_payment_response_statusCode_is_not_SUCCESS()
        {
            mockPayuService.Setup(m => m.AcceptPaymentAsync(It.IsAny<string>())).ReturnsAsync(new DataAccessLayer.Models.Payu.AcceptPayment { status = new DataAccessLayer.Models.Payu.Status { statusCode = "Error" } });

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.AcceptPaymentConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
        }

        [Test]
        public async Task AcceptPaymentConfirm_saves_payment_and_redirects_to_Details_action_in_Order_controller_if_accept_payment_response_statusCode_is_SUCCESS()
        {
            mockPayuService.Setup(m => m.AcceptPaymentAsync(It.IsAny<string>())).ReturnsAsync(new DataAccessLayer.Models.Payu.AcceptPayment { status = new DataAccessLayer.Models.Payu.Status { statusCode = "SUCCESS" } });

            PayuController controller = new PayuController(mockPayuService.Object, mockOrderService.Object, mapper);

            var result = await controller.AcceptPaymentConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Order", result.RouteValues["controller"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.SavePaymentAsync(10, "COMPLETED", It.IsAny<string>()), Times.Once);
        }
        #endregion
    }
}
