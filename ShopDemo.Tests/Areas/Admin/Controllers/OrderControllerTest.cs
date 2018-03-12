using AutoMapper;
using DataAccessLayer.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using NUnit.Framework;
using PagedList;
using Postal;
using Rotativa.MVC;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class OrderControllerTest
    {
        Mock<IOrderService> mockOrderService;
        Mock<IDeliveryService> mockDeliveryService;
        Mock<IEmailService> mockEmailService;
        IMapper mapper;
        Mock<IBackgroundJobClient> mockJobClient;

        List<Order> ordersList;

        Order orderWithDetails;

        [SetUp]
        public void SetUp()
        {
            mockOrderService = new Mock<IOrderService>();
            mockDeliveryService = new Mock<IDeliveryService>();
            mockEmailService = new Mock<IEmailService>();
            mockJobClient = new Mock<IBackgroundJobClient>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrderProfile());
                cfg.AddProfile(new OrderDetailProfile());
                cfg.AddProfile(new UserProfile());
            });
            mapper = config.CreateMapper();

            ordersList = new List<Order>();
            ordersList.Add(new Order { OrderId = 1, OrderDate = new DateTime(2017, 12, 20, 15, 32, 16), OrderRealizationDate = null, OrderStatus = OrderStatus.Uncompleted, TotalAmount = 234, UserId = 1, DeliveryId = 1 });
            ordersList.Add(new Order { OrderId = 2, OrderDate = new DateTime(2017, 12, 16, 14, 32, 16), OrderRealizationDate = new DateTime(2017, 12, 20, 15, 32, 16), OrderStatus = OrderStatus.Completed, TotalAmount = 123, UserId = 1, DeliveryId = 1 });
            ordersList.Add(new Order { OrderId = 3, OrderDate = new DateTime(2017, 10, 22, 11, 22, 16), OrderRealizationDate = null, OrderStatus = OrderStatus.Uncompleted, TotalAmount = 55, UserId = 1, DeliveryId = 1 });
            ordersList.Add(new Order { OrderId = 4, OrderDate = new DateTime(2017, 12, 20, 14, 32, 16), OrderRealizationDate = null, OrderStatus = OrderStatus.WaitingForPayment, TotalAmount = 12, UserId = 2, DeliveryId = 1 });
            ordersList.Add(new Order { OrderId = 5, OrderDate = new DateTime(2017, 12, 20, 16, 32, 16), OrderRealizationDate = null, OrderStatus = OrderStatus.CancelledByCustomer, TotalAmount = 78, UserId = 2, DeliveryId = 1 });

            Address address = new Address { UserId = 1, City = "Warszawa", HouseNumber = "10a" };
            ApplicationUser user = new ApplicationUser { Id = 1, FirstName = "Jan", LastName = "Kowalski", Address = address };
            Delivery delivery = new Delivery { DeliveryId = 1, IsActive = true, PaymentOption = PaymentOptions.CashOnDelivery, Price = 0 };
            List<OrderDetails> orderDetails = new List<OrderDetails> { new OrderDetails { OrderDetailsId = 1, ProductId = 1, OrderId = 1, ProductQuantity = 2, ProductUnitPrice = 4, Total = 8, Product = new Product { ProductId = 1, Name = "TestProduct" } } };
            orderWithDetails = new Order { User = user, OrderDetails = orderDetails, DeliveryOption = delivery, OrderId = 1, OrderStatus = OrderStatus.Uncompleted, TotalAmount = 50 };
        }

        #region GetAllOrders
        [Test]
        public async Task GetAllOrders_returns_partial_view_with_paged_orders()
        {
            mockOrderService.Setup(m => m.PageAllAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(), It.IsAny<string[]>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(ordersList.Where(o => o.OrderStatus == OrderStatus.Uncompleted).OrderBy(o => o.OrderDate));

            mockOrderService.Setup(m => m.EntitiesCountAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(It.IsAny<int>());

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetAllOrders(null, 0, OrderStatus.Uncompleted) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(StaticPagedList<OrdersIndexViewModel>), result.Model);
            Assert.AreEqual("_OrdersListPartial", result.ViewName);
            var model = (StaticPagedList<OrdersIndexViewModel>)result.Model;
            Assert.IsTrue(model.Count == 2);
            Assert.That(model, Is.Ordered.By("OrderDate"));
        }

        [Test]
        public async Task GetAllOrders_search_orders_by_orderId_and_returns_partial_view_with_paged_orders()
        {
            mockOrderService.Setup(m => m.PageAllAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(), It.IsAny<string[]>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(ordersList.Where(o => o.OrderStatus == OrderStatus.Uncompleted && o.OrderId == 1).OrderBy(o => o.OrderDate));

            mockOrderService.Setup(m => m.EntitiesCountAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(It.IsAny<int>());

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetAllOrders(null, 1, OrderStatus.Uncompleted) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(StaticPagedList<OrdersIndexViewModel>), result.Model);
            Assert.AreEqual("_OrdersListPartial", result.ViewName);
            var model = (StaticPagedList<OrdersIndexViewModel>)result.Model;
            Assert.IsTrue(model.Count == 1);
        }
        #endregion

        #region Details
        [Test]
        public async Task Details_returns_BadRequest_if_orderId_is_not_provided()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Details_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_pass_error_message_from_TempData_to_ViewBag()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);
            var temp = new TempDataDictionary();
            temp.Add("ErrorMessage", "Test error");

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.TempData = temp;

            var result = await controller.Details(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Test error", result.ViewBag.ErrorMessage);
        }

        [Test]
        public async Task Details_pass_success_message_from_TempData_to_ViewBag()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);
            var temp = new TempDataDictionary();
            temp.Add("SuccessMessage", "Test error");

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.TempData = temp;

            var result = await controller.Details(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Test error", result.ViewBag.SuccessMessage);
        }

        [Test]
        public async Task Details_returns_view_with_order_details_data()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details(10) as ViewResult;
            var resultModel = (OrderDetailViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual("Warszawa", resultModel.Address.City);
            Assert.AreEqual("10a", resultModel.Address.HouseNumber);
            Assert.AreEqual("Jan", resultModel.CustomerData.FirstName);
            Assert.AreEqual("Kowalski", resultModel.CustomerData.LastName);
            Assert.AreEqual(1, resultModel.OrderBaseData.OrderId);
            Assert.AreEqual(50, resultModel.OrderBaseData.TotalAmount);
            Assert.AreEqual("TestProduct", resultModel.OrderDetails[0].ProductName);
        }
        #endregion

        #region RealizeOrder
        [Test]
        public async Task RealizeOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task RealizeOrder_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task RealizeOrder_redirects_to_Index_action_and_pass_message_by_temp_data_if_order_status_is_DuringRealization()
        {
            Order order = new Order { OrderId = 10, OrderStatus = OrderStatus.DuringRealization };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(order);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(controller.TempData["message"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task RealizeOrder_sets_order_status_to_DuringRealization()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);
            mockOrderService.Setup(m => m.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>())).Returns(Task.FromResult(0));

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder(1) as ViewResult;

            Assert.IsNotNull(result);
            mockOrderService.Verify(m => m.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
        }

        [Test]
        public async Task RealizeOrder_creates_new_hangfire_scheduled_job_and_pass_jobId_into_TempData()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "ChangeStatusToUncompleted"), It.IsAny<ScheduledState>())).Returns("jobId");

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder(1) as ViewResult;

            Assert.IsNotNull(result);
            mockJobClient.Verify(m => m.Create(It.Is<Job>(job => job.Method.Name == "ChangeStatusToUncompleted"), It.IsAny<ScheduledState>()), Times.Once);
            Assert.AreEqual("jobId", controller.TempData["realizeOrderJobId"]);
        }

        [Test]
        public async Task RealizeOrder_returns_order_data()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(orderWithDetails);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.RealizeOrder(1) as ViewResult;
            var resultModel = (RealizeOrderViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultModel.Delivery.DeliveryId);
            Assert.AreEqual(PaymentOptionsViewModel.CashOnDelivery, resultModel.Delivery.PaymentOption);
            Assert.AreEqual(1, resultModel.OrderBaseData.OrderId);
            Assert.AreEqual(50, resultModel.OrderBaseData.TotalAmount);
        }
        #endregion

        #region RealizeOrderConfirm
        [Test]
        public async Task RealizeOrderConfirm_returns_model_state_error_if_model_state_is_invalid()
        {
            Order order = new Order();
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(order);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.RealizeOrderConfirm(new RealizeOrderViewModel()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
            Assert.IsNotNull(result.Model);
        }

        [Test]
        public async Task RealizeOrderConfirm_change_order_status_sent_email_and_redirects_to_RealizeOrderSuccess_action_if_model_state_is_valid()
        {
            OrdersIndexViewModel orderBaseData = new OrdersIndexViewModel { OrderId = 1 };

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            mockOrderService.Setup(m => m.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>())).Returns(Task.FromResult(true));
            mockOrderService.Setup(m => m.FindUserEmailByOrderId(It.IsAny<int>())).ReturnsAsync(It.IsAny<string>());
            mockEmailService.Setup(m => m.SendAsync(new OrderInfoEmail()));

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.RealizeOrderConfirm(new RealizeOrderViewModel { OrderBaseData = orderBaseData }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("RealizeOrderSuccess", result.RouteValues["action"]);
            Assert.AreEqual(1, result.RouteValues["orderId"]);

            mockOrderService.Verify(m => m.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<OrderInfoEmail>()), Times.Once);
        }
        #endregion

        [Test]
        public void EnableOrderToRealization_change_order_status_to_uncompleted()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = controller.EnableOrderToRealization(It.IsAny<int>());

            mockOrderService.Verify(m => m.ChangeStatusToUncompleted(It.IsAny<int>()), Times.Once);
        }

        #region PrintBill
        [Test]
        public async Task PrintBill_returns_BadRequest_if_orderId_is_not_provided()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.PrintBill() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task PrintBill_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.PrintBill(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task PrintBill_returns_view_as_PDF()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Order { DeliveryOption = new Delivery() });

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.PrintBill(10);

            Assert.IsNotNull(result);
            Assert.AreEqual("Rotativa.MVC.ViewAsPdf", result.GetType().ToString());
            Assert.AreEqual("OrderBill", ((ViewAsPdf)result).ViewName);
        }
        #endregion

        #region CancelOrder
        [Test]
        public async Task CancelOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CancelOrder_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullOrder);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task CancelOrder_redirects_to_Details_action_and_pass_error_message_by_temp_data_if_order_status_is_CancelledByAdmin()
        {
            Order order = new Order { OrderId = 10, OrderStatus = OrderStatus.CancelledByAdmin };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(order);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(controller.TempData["ErrorMessage"]);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
        }

        [Test]
        public async Task CancelOrder_returns_view_with_order_data()
        {
            Order order = new Order { OrderId = 10, OrderStatus = OrderStatus.Uncompleted, TotalAmount = 100, DeliveryOption = new Delivery { PaymentOption = PaymentOptions.PaymentByTransfer } };
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(order);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder(10) as ViewResult;
            var resultModel = (AdminCancelOrder)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(OrderStatusViewModel.Uncompleted, resultModel.OrderStatus);
            Assert.AreEqual(order.TotalAmount, resultModel.TotalAmount);
            Assert.AreEqual(order.DeliveryOption.PaymentOption, resultModel.PaymentOption);
        }

        [Test]
        public async Task CancelOrder_returns_model_state_error_if_model_state_is_not_valid()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.CancelOrder(It.IsAny<AdminCancelOrder>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task CancelOrder_cancels_order_and_redirects_to_Refund_action_in_Payu_controller_if_order_status_is_Uncompleted_and_payment_option_is_PaymentByTransfer()
        {
            mockOrderService.Setup(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>())).Returns(Task.FromResult(0));

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder(new AdminCancelOrder { OrderId = 10, PaymentOption = PaymentOptions.PaymentByTransfer, OrderStatus = OrderStatusViewModel.Uncompleted }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Refund", result.RouteValues["action"]);
            Assert.AreEqual("Payu", result.RouteValues["controller"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
        }

        [Test]
        public async Task CancelOrder_pass_message_with_cancel_order_reason_into_TempData_if_order_status_is_Uncompleted_and_payment_option_is_PaymentByTransfer()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.CancelOrder(new AdminCancelOrder { Message = "Test message", OrderId = 10, PaymentOption = PaymentOptions.PaymentByTransfer, OrderStatus = OrderStatusViewModel.Uncompleted }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Test message", controller.TempData["CancelOrderReason"]);
        }

        [Test]
        public async Task CancelOrder_cancels_order_sent_email_and_redirects_to_Details_action()
        {
            mockOrderService.Setup(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>())).Returns(Task.FromResult(0));
            mockEmailService.Setup(m => m.SendAsync(new OrderInfoEmail()));

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());


            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CancelOrder(new AdminCancelOrder { OrderId = 10, PaymentOption = PaymentOptions.CashOnDelivery, OrderStatus = OrderStatusViewModel.Uncompleted }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockOrderService.Verify(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<OrderInfoEmail>()), Times.Once);
        }
        #endregion

        [Test]
        public async Task CancelOrderSuccess_sent_email_about_order_cancellation_and_redirects_to_Details_action()
        {
            mockEmailService.Setup(m => m.SendAsync(new OrderInfoEmail()));

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CancelOrderSuccess(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["orderId"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<OrderInfoEmail>()), Times.Once);
        }

        #region UserOrders
        [Test]
        public async Task UserOrders_returns_BadRequest_if_userId_is_not_provided()
        {
            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.UserOrders("email") as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UserOrders_returns_view_with_user_orders_grouped_by_order_status()
        {
            mockOrderService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(ordersList.Where(u => u.UserId == 1));

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            string email = "email";
            byte[] bytes = Encoding.UTF8.GetBytes(email);
            string encodedEmail = Convert.ToBase64String(bytes);
            var result = await controller.UserOrders(encodedEmail, 1) as ViewResult;
            var resultModel = ((IEnumerable<UserOrdersListViewModel>)result.Model).ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(OrderStatusViewModel.Uncompleted, resultModel[0].OrderStatus);
            Assert.AreEqual(2, resultModel[0].UserOrders.Count);
            Assert.AreEqual(OrderStatusViewModel.Completed, resultModel[1].OrderStatus);
            Assert.AreEqual(1, resultModel[1].UserOrders.Count);
        }
        #endregion

        [Test]
        public async Task GetOrdersForSelectedMonth_returns_paged_orders_for_selected_month()
        {
            mockOrderService.Setup(m => m.PageAllAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<string[]>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(ordersList);

            OrderController controller = new OrderController(mockOrderService.Object, mockDeliveryService.Object, mockEmailService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetOrdersForSelectedMonth("grudzień", 2017, null) as ViewResult;

            Assert.IsNotNull(result);
            Assert.That(result.Model, Is.InstanceOf(typeof(StaticPagedList<OrdersIndexViewModel>)));
        }
    }
}