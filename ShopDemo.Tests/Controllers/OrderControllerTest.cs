using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Service_Layer.Services;
using DataAccessLayer.Models;
using ShopDemo.Controllers;
using AutoMapper;
using System.Web.Mvc;
using System;
using System.Linq;
using ShopDemo.ViewModels;
using System.Linq.Expressions;
using ShopDemo.AutoMapperProfiles;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Postal;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class OrderControllerTest
    {
        Mock<IOrderService> mockOrderService;
        Mock<IDeliveryService> mockDeliveryService;
        Mock<ICartService> mockCartService;
        Mock<IEmailService> mockEmailService;

        Order order;
        Delivery delivery;
        List<OrderDetails> orderDetails;
        Dictionary<int, string> orderErrors;
        List<Order> orders;
        OrderController controller;
        Product product1;
        Product product2;

        IMapper mapper;

        Mock<ControllerContext> mockControllerContext;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrderDetailProfile());
                cfg.AddProfile(new UserProfile());
                cfg.AddProfile(new OrderProfile());

            });
            mapper = config.CreateMapper();

            mockOrderService = new Mock<IOrderService>();
            mockDeliveryService = new Mock<IDeliveryService>();
            mockCartService = new Mock<ICartService>();
            mockEmailService = new Mock<IEmailService>();

            delivery = new Delivery { DeliveryId = 1, Option = "Odbiór osobisty", PaymentOption = PaymentOptions.NotApplicable, Price = 10, RealizationTimeInDays = 1 };
            Address address = new Address { City = "Warszawa", HouseNumber = "10a", Street = "Wiejska", ZipCode = "44-444" };
            ApplicationUser user = new ApplicationUser { FirstName = "Jan", LastName = "Kowalski", AccountIsEnabled = true, Address = address };
            product1 = new Product { ProductId = 1, Name = "Product1" };
            product2 = new Product { ProductId = 2, Name = "Product2" };

            orderDetails = new List<OrderDetails>
            {
                new OrderDetails { OrderId=1, OrderDetailsId=1, ProductId=1, Product=product1, ProductQuantity=1, ProductUnitPrice = 20, Total = 20 },
                new OrderDetails { OrderId=1, OrderDetailsId=2, ProductId=2, Product=product2, ProductQuantity=1, ProductUnitPrice = 20, Total = 20 }
            };

            order = new Order
            {
                DeliveryId = 1,
                DeliveryOption = delivery,
                UserId = 1,
                User = user,
                OrderDate = DateTime.Now,
                TotalAmount = 40,
                OrderId = 1,
                OrderStatus = OrderStatus.Uncompleted,
                Removed = false,
                OrderDetails = orderDetails
            };

            orderErrors = new Dictionary<int, string>();
            orders = new List<Order>
            {
                new Order { OrderId = 1, DeliveryId = 1, UserId = 1, OrderDate = DateTime.Now, OrderStatus = OrderStatus.Completed, Removed = true, TotalAmount = 124 },
                new Order { OrderId = 2, DeliveryId = 2, UserId = 2, OrderDate = DateTime.Now, OrderStatus = OrderStatus.Completed,Removed = false, TotalAmount = 246 },
                new Order { OrderId = 3, DeliveryId = 3, UserId = 1, OrderDate = DateTime.Now, OrderStatus = OrderStatus.Uncompleted,Removed = false, TotalAmount = 489 },
                new Order { OrderId = 4, DeliveryId = 1, UserId = 1, OrderDate = DateTime.Now, OrderStatus = OrderStatus.WaitingForPayment,Removed = false, TotalAmount = 726 },
                new Order { OrderId = 5, DeliveryId = 2, UserId = 3, OrderDate = DateTime.Now, OrderStatus = OrderStatus.CancelledByCustomer,Removed = false, TotalAmount = 254 },
                new Order { OrderId = 6, DeliveryId = 3, UserId = 1, OrderDate = DateTime.Now, OrderStatus = OrderStatus.Uncompleted,Removed = false, TotalAmount = 1248 }
            };

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Jan"));
            claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "1"));

            var genericIdentity = new GenericIdentity("");
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, new string[] { });

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = new Uri("http://shopdemo.pl/confirmemail");
            mockHttpRequest.Setup(m => m.Url).Returns(uri);

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(x => x.User).Returns(genericPrincipal);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);
            mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);
        }

        #region Index
        [Test]
        public async Task Index_returns_all_user_orders_splited_on_groups_by_order_status_without_orders_removed_form_list()
        {
            var userOrders = orders.Where(u => u.UserId == 1 && u.Removed == false).AsEnumerable();

            mockOrderService.Setup(o => o.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(userOrders));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Index() as ViewResult;

            IEnumerable<UserOrdersListViewModel> ordersModel = (IEnumerable<UserOrdersListViewModel>)result.Model;

            UserOrderViewModel[] uncompletedOrders = ordersModel.Where(o => o.OrderStatus == OrderStatusViewModel.Uncompleted).SelectMany(i => i.UserOrders).ToArray();
            UserOrderViewModel[] waitingForPaymentOrders = ordersModel.Where(o => o.OrderStatus == OrderStatusViewModel.WaitingForPayment).SelectMany(i => i.UserOrders).ToArray();

            Assert.That(ordersModel, Is.Not.Null);
            Assert.IsTrue(ordersModel.Count() == 2);//two groups
            Assert.IsTrue(uncompletedOrders.Count() == 2);
            Assert.AreEqual(uncompletedOrders[0].OrderId, 3);
            Assert.AreEqual(uncompletedOrders[1].OrderId, 6);
            Assert.IsTrue(waitingForPaymentOrders.Count() == 1);
            Assert.AreEqual(waitingForPaymentOrders[0].OrderId, 4);
        }
        #endregion

        #region Details
        [Test]
        public async Task Details_returns_BadRequest_if_orderId_is_not_provided()
        {
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.Details() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Details_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(nullOrder));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Details(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_returns_correct_order_data()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(order));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.Details(10) as ViewResult;

            OrderDetailViewModel resultModel = (OrderDetailViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("Warszawa", resultModel.Address.City);
            Assert.AreEqual("Jan", resultModel.CustomerData.FirstName);
            Assert.AreEqual(40, resultModel.OrderBaseData.TotalAmount);
            Assert.AreEqual(3, resultModel.OrderDetails.Count()); //delivery option added on list as product
            Assert.AreEqual("Odbiór osobisty", resultModel.OrderDetails[2].ProductName);
        }
        #endregion

        #region OrderSummaryPreview
        [Test]
        public async Task OrderSummaryPreview_redirects_to_cart_when_cart_is_empty()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = null;
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.OrderSummaryPreview() as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("GetCart"));
            Assert.That(result.RouteValues["controller"], Is.EqualTo("Cart"));
        }

        [Test]
        public async Task OrderSummaryPreview_returns_BadRequest_if_deliveryId_is_not_provided()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.OrderSummaryPreview() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task OrderSummaryPreview_returns_NotFound_if_delivery_is_null()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Delivery nullDelivery = null;
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(nullDelivery));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.OrderSummaryPreview(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task OrderSummaryPreview_returns_correct_data()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>
            {
                new CartItem {  ProductId= 1, Product = new Product { ProductId=1, Name="Produkt1" }, ProductCount = 1, ProductPrice = 10 },
                new CartItem {  ProductId= 2, Product = new Product { ProductId=2, Name="Produkt2" }, ProductCount = 2, ProductPrice = 50 },
                new CartItem {  ProductId= 3, Product = new Product { ProductId=3, Name="Produkt3" }, ProductCount = 5, ProductPrice = 20 }
            };
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(delivery));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.OrderSummaryPreview(10) as ViewResult;

            OrderSummaryViewModel resultModel = (OrderSummaryViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.That(resultModel.DeliveryOption, Is.Not.Null);
            Assert.AreEqual("Odbiór osobisty", resultModel.DeliveryOption.Option);
            Assert.That(resultModel.Products, Is.Not.Null);
            Assert.AreEqual("Produkt1", resultModel.Products[0].ProductName);
            Assert.AreEqual(2, resultModel.Products[1].ProductId);
            Assert.AreEqual(220, resultModel.TotalPrice); //TotalPrice = productsPrice + deliveryPrice
        }
        #endregion

        #region CreateOrder
        [Test]
        public async Task CreateOrder_redirects_to_SelectDeliveryOption_if_TempData_with_deliveryId_is_null()
        {
            var temp = new TempDataDictionary();
            temp.Add("deliveryId", null);

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.TempData = temp;
            var result = await controller.CreateOrder() as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("SelectDeliveryOption"));
            Assert.That(result.RouteValues["controller"], Is.EqualTo("Delivery"));
        }

        [Test]
        public async Task CreateOrder_redirects_to_OrderFailure_if_create_new_order_method_returs_error()
        {
            var temp = new TempDataDictionary();
            temp.Add("deliveryId", It.IsAny<int>());

            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(delivery));

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Dictionary<int, string> orderErrors = new Dictionary<int, string>();
            orderErrors.Add(1, "TestError");

            mockOrderService.Setup(m => m.CreateNewOrderAsync(It.IsAny<Order>())).Returns(Task.FromResult(orderErrors));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;
            controller.TempData = temp;

            var result = await controller.CreateOrder() as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("OrderFailure"));
        }

        [Test]
        public async Task CreateOrder_redirects_to_OrderSuccess_if_create_new_order_is_success()
        {
            var temp = new TempDataDictionary();
            temp.Add("deliveryId", It.IsAny<int>());

            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(delivery));

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Dictionary<int, string> orderErrors = new Dictionary<int, string>();

            mockOrderService.Setup(m => m.CreateNewOrderAsync(It.IsAny<Order>())).Returns(Task.FromResult(orderErrors));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;
            controller.TempData = temp;

            var result = await controller.CreateOrder() as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("OrderSuccess"));
            mockCartService.Verify(m => m.EmptyCart(), Times.Once);
        }
        #endregion

        #region GetUncompletedOrders
        [Test]
        public async Task GetUncompletedOrders_returns_empty_view_result_if_order_delivery_payment_option_is_PaymentByTransfer()
        {
            Delivery delivery = new Delivery { DeliveryId=1, PaymentOption = PaymentOptions.PaymentByTransfer };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.GetUncompletedOrders(It.IsAny<int>());

            Assert.IsInstanceOf<EmptyResult>(result);
        }

        [Test]
        public async Task GetUncompletedOrders_returns_partial_view_if_uncompleted_orders_exists()
        {
            Delivery delivery = new Delivery { DeliveryId = 1, PaymentOption = PaymentOptions.CashOnDelivery };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            var userOrders = orders.Where(o => o.UserId == 1 && o.DeliveryId == 1 && (o.OrderStatus == OrderStatus.Uncompleted || o.OrderStatus == OrderStatus.WaitingForPayment)).AsEnumerable();
            mockOrderService.Setup(o => o.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(userOrders));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.GetUncompletedOrders(It.IsAny<int>()) as PartialViewResult;

            Assert.AreEqual("_UncompletedOrdersPartial", result.ViewName);
            Assert.IsInstanceOf<IEnumerable<UncompleteOrdersViewModel>>(result.ViewData.Model);
        }

        [Test]
        public async Task GetUncompletedOrders_returns_empty_view_result_if_uncompleted_orders_not_exists()
        {
            Delivery delivery = new Delivery { DeliveryId = 1, PaymentOption = PaymentOptions.CashOnDelivery };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            var userOrders = orders.Where(o => o.UserId == 3 && o.DeliveryId == 2 && (o.OrderStatus == OrderStatus.Uncompleted || o.OrderStatus == OrderStatus.WaitingForPayment)).AsEnumerable();
            mockOrderService.Setup(o => o.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(userOrders));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.GetUncompletedOrders(It.IsAny<int>());

            Assert.IsInstanceOf<EmptyResult>(result);
        }
        #endregion

        #region UpdateOrderSummaryPreview
        public async Task UpdateOrderSummaryPreview_redirects_to_cart_when_cart_is_empty()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = null;
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrderSummaryPreview() as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("GetCart"));
            Assert.That(result.RouteValues["controller"], Is.EqualTo("Cart"));
        }

        [Test]
        public async Task UpdateOrderSummaryPreview_returns_BadRequest_if_deliveryId_or_orderId_are_not_provided()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrderSummaryPreview() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateOrderSummaryPreview_returns_NotFound_if_delivery_is_null()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Delivery nullDelivery = null;
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(nullDelivery));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrderSummaryPreview(10, 10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UpdateOrderSummaryPreview_returns_NotFound_if_order_is_null()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(delivery));
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(nullOrder));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrderSummaryPreview(10, 10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task UpdateOrderSummaryPreview_returns_correct_data()
        {
            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>
            {
                new CartItem {  ProductId= 1, Product = new Product { ProductId=1, Name="CartProduct1" }, ProductCount = 1, ProductPrice = 10 },
                new CartItem {  ProductId= 2, Product = new Product { ProductId=2, Name="CartProduct2" }, ProductCount = 2, ProductPrice = 50 },
                new CartItem {  ProductId= 3, Product = new Product { ProductId=3, Name="CartProduct3" }, ProductCount = 5, ProductPrice = 20 }
            };
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            mockOrderService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(order));
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(delivery));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrderSummaryPreview(10, 10) as ViewResult;

            UpdateOrderSummaryViewModel resultModel = (UpdateOrderSummaryViewModel)result.Model;

            Assert.That(result, Is.Not.Null);

            Assert.That(resultModel.OrderBaseData, Is.Not.Null);
            Assert.AreEqual(1, resultModel.OrderBaseData.OrderId);

            Assert.That(resultModel.DeliveryOption, Is.Not.Null);
            Assert.AreEqual("Odbiór osobisty", resultModel.DeliveryOption.Option);

            Assert.That(resultModel.Products, Is.Not.Null);
            Assert.AreEqual("CartProduct1", resultModel.Products[0].ProductName);
            Assert.AreEqual(2, resultModel.Products[1].ProductId);

            Assert.That(resultModel.ProductsFromOrder, Is.Not.Null);
            Assert.AreEqual("Product1", resultModel.ProductsFromOrder[0].ProductName);

            Assert.AreEqual(260, resultModel.TotalPrice); //TotalPrice = productsExistingInOrders + newProducts + deliveryPrice
        }
        #endregion

        #region UpdateOrder
        [Test]
        public async Task UpdateOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrder() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task UpdateOrder_redirects_to_OrderSummaryPreview_when_order_status_change()
        {
            order.OrderStatus = OrderStatus.CancelledByAdmin;
            order.DeliveryId = 10;
            mockOrderService.Setup(o => o.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(order));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            var result = await controller.UpdateOrder(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("OrderSummaryPreview"));
            Assert.That(result.RouteValues["deliveryId"], Is.EqualTo(10));
        }

        [Test]
        public async Task UpdateOrder_redirects_to_OrderFailure_if_update_order_method_returs_error()
        {
            Order order = new Order { OrderStatus = OrderStatus.Uncompleted };
            mockOrderService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(order));

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Dictionary<int, string> orderErrors = new Dictionary<int, string>();
            orderErrors.Add(1, "TestError");

            mockOrderService.Setup(m => m.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<IEnumerable<OrderDetails>>())).Returns(Task.FromResult(orderErrors));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrder(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("OrderFailure"));
        }

        [Test]
        public async Task UpdateOrder_redirects_to_Index_if_update_is_success()
        {
            Order order = new Order { OrderStatus = OrderStatus.Uncompleted };
            mockOrderService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).Returns(Task.FromResult(order));

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>();
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            Dictionary<int, string> orderErrors = new Dictionary<int, string>();

            mockOrderService.Setup(m => m.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<IEnumerable<OrderDetails>>())).Returns(Task.FromResult(orderErrors));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.UpdateOrder(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
            mockCartService.Verify(m => m.EmptyCart(), Times.Once);
        }
        #endregion

        #region OrderFailure
        [Test]
        public void OrderFailure_returns_NotFound_if_TempData_with_order_errors_is_null()
        {
            var temp = new TempDataDictionary();
            temp.Add("orderErrors", null);

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.TempData = temp;

            var result = controller.OrderFailure() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void OrderFailure_returns_correcta_data()
        {
            Dictionary<int, string> orderErrors = new Dictionary<int, string>();
            orderErrors.Add(0, "Change order status error");
            orderErrors.Add(1, "Product error");

            var temp = new TempDataDictionary();
            temp.Add("orderErrors", orderErrors);

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem>
            {
                new CartItem {  ProductId= 1, Product = new Product { ProductId=1, Name="CartProduct1" }, ProductCount = 1, ProductPrice = 10 },
                new CartItem {  ProductId= 2, Product = new Product { ProductId=2, Name="CartProduct2" }, ProductCount = 2, ProductPrice = 50 }
            };
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.TempData = temp;

            var result = controller.OrderFailure() as ViewResult;

            OrderErrorViewModel resultModel = (OrderErrorViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("Change order status error", resultModel.OrderStatusError);
            Assert.AreEqual("CartProduct1", resultModel.ProductErrors[0].ProductName);
            Assert.AreEqual("Product error", resultModel.ProductErrors[0].Error);
        }
        #endregion

        #region OrderSuccess
        [Test]
        public async Task OrderSuccess_sent_email_about_new_order_and_redirects_to_CreateNewOrder_action_in_Payu_controller_if_payment_option_is_pay_by_transfer()
        {
            mockEmailService.Setup(m => m.SendAsync(new OrderInfoEmail()));
            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            var temp = new TempDataDictionary();
            temp.Add("paymentOption", PaymentOptions.PaymentByTransfer);

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;
            controller.TempData = temp;

            var result = await controller.OrderSuccess(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("CreateNewOrder"));
            Assert.That(result.RouteValues["controller"], Is.EqualTo("Payu"));
            Assert.That(result.RouteValues["orderId"], Is.EqualTo(10));
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<OrderInfoEmail>()), Times.Once);
        }
        #endregion

        #region CancelOrder
        [Test]
        public async Task CancelOrder_returns_BadRequest_if_orderId_is_not_provided()
        {
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.CancelOrder() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CancelOrder_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(nullOrder));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CancelOrder(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task CancelOrder_returns_correct_data()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(order));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CancelOrder(10) as ViewResult;

            OrdersIndexViewModel resultModel = (OrdersIndexViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(1, resultModel.OrderId);
            Assert.AreEqual(OrderStatusViewModel.Uncompleted, resultModel.OrderStatus);
            Assert.AreEqual(40, resultModel.TotalAmount);
        }

        [Test]
        public async Task CancelOrderConfirm_sent_email_about_cancelled_order_and_redirects_to_Index_action_if_cancel_order_is_success()
        {
            mockOrderService.Setup(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>())).Returns(Task.FromResult(0));
            mockEmailService.Setup(m => m.SendAsync(new OrderInfoEmail()));
            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns(It.IsAny<string>());

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.CancelOrderConfirm(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            mockOrderService.Verify(m => m.CancelOrderAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Once);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<OrderInfoEmail>()), Times.Once);
        }
        #endregion

        #region RemoveOrderFromList
        [Test]
        public async Task RemoveOrderFromList_returns_BadRequest_if_orderId_is_not_provided()
        {
            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.RemoveOrderFromList() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task RemoveOrderFromList_returns_NotFound_if_order_is_null()
        {
            Order nullOrder = null;
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(nullOrder));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.RemoveOrderFromList(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task RemoveOrderFromList_returns_correct_data()
        {
            mockOrderService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Order, bool>>>())).Returns(Task.FromResult(order));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);
            controller.ControllerContext = mockControllerContext.Object;

            var result = await controller.RemoveOrderFromList(10) as ViewResult;

            OrdersIndexViewModel resultModel = (OrdersIndexViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(1, resultModel.OrderId);
            Assert.AreEqual(OrderStatusViewModel.Uncompleted, resultModel.OrderStatus);
            Assert.AreEqual(40, resultModel.TotalAmount);
        }

        [Test]
        public async Task RemoveOrderFromListConfirm_set_order_Removed_property_to_true_and_redirect_to_Index()
        {
            mockOrderService.Setup(m => m.RemoveOrderFormCustomerOrdersListAsync(It.IsAny<int>())).Returns(Task.FromResult(0));

            controller = new OrderController(mockCartService.Object, mockDeliveryService.Object, mockOrderService.Object, mapper, mockEmailService.Object);

            var result = await controller.RemoveOrderFromListConfirm(10) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            mockOrderService.Verify(m => m.RemoveOrderFormCustomerOrdersListAsync(It.IsAny<int>()), Times.Once);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
        }
        #endregion
    }
}