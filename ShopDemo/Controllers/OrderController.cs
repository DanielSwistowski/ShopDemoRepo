using AutoMapper;
using DataAccessLayer.Models;
using Microsoft.AspNet.Identity;
using Postal;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartService cartService;
        private readonly IDeliveryService deliveryService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        public OrderController(ICartService cartService, IDeliveryService deliveryService, IOrderService orderService, IMapper mapper, IEmailService emailService)
        {
            this.cartService = cartService;
            this.deliveryService = deliveryService;
            this.orderService = orderService;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        public async Task<ActionResult> Index()
        {
            if (TempData["message"] != null)
                ViewBag.Message = (string)TempData["message"];

            int userId = User.Identity.GetUserId<int>();
            var orders = await orderService.GetAllAsync(u => u.UserId == userId && u.Removed == false);

            IEnumerable<UserOrdersListViewModel> ordersViewModel = orders.GroupBy(o => o.OrderStatus)
                .Select(g => new UserOrdersListViewModel { OrderStatus = (OrderStatusViewModel)g.Key, UserOrders = mapper.Map<List<UserOrderViewModel>>(g) });

            return View(ordersViewModel);
        }

        public async Task<ActionResult> Details(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int userId = User.Identity.GetUserId<int>();

            string[] includedProperties = { "DeliveryOption", "User", "User.Address", "OrderDetails", "OrderDetails.Product" };
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId && o.UserId == userId, includedProperties);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            OrderDetailViewModel model = mapper.Map<OrderDetailViewModel>(order);
            model.OrderBaseData = mapper.Map<OrdersIndexViewModel>(order);

            return View(model);
        }

        public async Task<ActionResult> OrderSummaryPreview(int deliveryId = 0)
        {
            IEnumerable<CartItem> cartItems = cartService.GetCart().CartItems;
            if (cartItems == null)
                return RedirectToAction("GetCart", "Cart");

            if (deliveryId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);
            if (delivery == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            TempData["deliveryId"] = deliveryId;

            DeliveryOptionsViewModel deliveryModel = mapper.Map<DeliveryOptionsViewModel>(delivery);
            List<CartItemViewModel> products = mapper.Map<List<CartItemViewModel>>(cartItems);

            OrderSummaryViewModel model = new OrderSummaryViewModel();
            model.DeliveryOption = deliveryModel;
            model.Products = products;

            if (TempData["error"] != null)
                ViewBag.Error = (string)TempData["error"];

            return View(model);
        }

        public async Task<ActionResult> CreateOrder()
        {
            if (TempData["deliveryId"] == null)
                return RedirectToAction("SelectDeliveryOption", "Delivery");

            int deliveryId = (int)TempData["deliveryId"];

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);

            int userId = User.Identity.GetUserId<int>();

            var cart = cartService.GetCart();

            ICollection<OrderDetails> orderDetails = mapper.Map<ICollection<OrderDetails>>(cart.CartItems);

            Order order = new Order();
            order.DeliveryId = deliveryId;
            order.UserId = userId;
            order.OrderDate = DateTime.Now;
            order.OrderStatus = delivery.PaymentOption == PaymentOptions.PaymentByTransfer ? OrderStatus.WaitingForPayment : OrderStatus.Uncompleted;
            order.OrderDetails = orderDetails;
            order.TotalAmount = orderDetails.Sum(p => p.Total) + delivery.Price;

            Dictionary<int, string> orderErrors = await orderService.CreateNewOrderAsync(order);

            if (orderErrors.Count != 0)
            {
                TempData["orderErrors"] = orderErrors;
                return RedirectToAction("OrderFailure");
            }

            cartService.EmptyCart();

            TempData["paymentOption"] = delivery.PaymentOption;

            return RedirectToAction("OrderSuccess", new { orderId = order.OrderId });
        }

        public async Task<ActionResult> GetUncompletedOrders(int deliveryId)
        {
            int userId = User.Identity.GetUserId<int>();
            var delivery = await deliveryService.FindByIdAsync(deliveryId);
            if (delivery.PaymentOption != PaymentOptions.PaymentByTransfer)
            {
                var orders = await orderService.GetAllAsync(o => o.UserId == userId && o.DeliveryId == deliveryId && (o.OrderStatus == OrderStatus.Uncompleted));

                if (orders.Count() > 0)
                {
                    IEnumerable<UncompleteOrdersViewModel> ordersModel = mapper.Map<IEnumerable<UncompleteOrdersViewModel>>(orders);
                    return PartialView("_UncompletedOrdersPartial", ordersModel);
                }
            }
            return new EmptyResult();
        }

        public async Task<ActionResult> UpdateOrderSummaryPreview(int orderId = 0, int deliveryId = 0)
        {
            IEnumerable<CartItem> cartItems = cartService.GetCart().CartItems;
            if (cartItems == null)
                return RedirectToAction("GetCart", "Cart");

            if (orderId == 0 || deliveryId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);
            if (delivery == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var order = await orderService.FindByIdAsync(orderId);
            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            UpdateOrderSummaryViewModel model = new UpdateOrderSummaryViewModel();
            model.OrderBaseData = mapper.Map<OrdersIndexViewModel>(order);
            model.ProductsFromOrder = mapper.Map<List<OrderDetailsViewModel>>(order.OrderDetails);
            model.DeliveryOption = mapper.Map<DeliveryOptionsViewModel>(delivery);
            model.Products = mapper.Map<List<CartItemViewModel>>(cartItems);

            return View(model);
        }

        public async Task<ActionResult> UpdateOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await orderService.FindByIdAsync(orderId);
            if (order.OrderStatus != OrderStatus.Uncompleted && order.OrderStatus != OrderStatus.WaitingForPayment)
            {
                TempData["error"] = "Nie można zaktualizować zamówienia, ponieważ jego status uległ zmianie.";
                return RedirectToAction("OrderSummaryPreview", new { deliveryId = order.DeliveryId });
            }

            var cart = cartService.GetCart();
            IEnumerable<OrderDetails> orderDetails = mapper.Map<IEnumerable<OrderDetails>>(cart.CartItems);

            Dictionary<int, string> orderErrors = await orderService.UpdateOrderAsync(orderId, orderDetails);

            if (orderErrors.Count != 0)
            {
                TempData["orderErrors"] = orderErrors;
                return RedirectToAction("OrderFailure");
            }

            cartService.EmptyCart();

            TempData["message"] = "Zamówienie nr " + order.OrderId + " zostało zaktualizowane.";

            return RedirectToAction("Index");
        }

        public ActionResult OrderFailure()
        {
            if (TempData["orderErrors"] == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            Dictionary<int, string> orderErrors = (Dictionary<int, string>)TempData["orderErrors"];

            string statusError = "";
            List<OrderFailureViewModel> products = new List<OrderFailureViewModel>();

            if (orderErrors.ContainsKey(0))
            {
                statusError = orderErrors.Where(p => p.Key == 0).Single().Value;
            }

            var cartItems = cartService.GetCart().CartItems;
            foreach (var item in cartItems)
            {
                OrderFailureViewModel failureModel = mapper.Map<OrderFailureViewModel>(item);
                if (orderErrors.ContainsKey(item.ProductId))
                {
                    failureModel.Error = orderErrors.Where(p => p.Key == item.ProductId).Single().Value;
                }
                products.Add(failureModel);
            }

            OrderErrorViewModel model = new OrderErrorViewModel();
            model.ProductErrors = products;
            model.OrderStatusError = statusError;

            return View(model);
        }

        public async Task<ActionResult> OrderSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;

            string emailAddress = User.Identity.GetUserName();
            string orderDetailsUrl = Url.Action("Details", "Order", new { orderId = orderId }, Request.Url.Scheme);
            var email = new OrderInfoEmail
            {
                To = emailAddress,
                Subject = EncodeStringHelpers.ConvertStringToUtf8("Nowe zamówienie"),
                Message = "Dziękujemy za złożenie nowego zamówienia. Kliknij w poniższy link, aby przejść na stronę sklepu i zobaczyć szczegóły dokonanego zamówienia.",
                OrderDetailsUrl = orderDetailsUrl
            };
            await emailService.SendAsync(email);

            if (TempData["paymentOption"] != null)
            {
                PaymentOptions paymentOption = (PaymentOptions)TempData["paymentOption"];
                if (paymentOption == PaymentOptions.PaymentByTransfer)
                    return RedirectToAction("CreateNewOrder", "Payu", new { orderId = orderId });
            }
            return View();
        }

        public async Task<ActionResult> CancelOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int userId = User.Identity.GetUserId<int>();

            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            OrdersIndexViewModel model = mapper.Map<OrdersIndexViewModel>(order);

            return View(model);
        }

        [HttpPost, ActionName("CancelOrder")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelOrderConfirm(int orderId)
        {
            await orderService.CancelOrderAsync(orderId, OrderStatus.CancelledByCustomer);

            string emailAddress = User.Identity.GetUserName();
            string orderDetailsUrl = Url.Action("Details", "Order", new { orderId = orderId }, Request.Url.Scheme);
            var email = new OrderInfoEmail
            {
                To = emailAddress,
                Subject = EncodeStringHelpers.ConvertStringToUtf8("Zamówienie anulowane"),
                Message = "Twoje zamówienie zostało anulowane. Kliknij w poniższy link, aby przejść na stronę sklepu i zobaczyć szczegóły anulowanego zamówienia.",
                OrderDetailsUrl = orderDetailsUrl
            };
            await emailService.SendAsync(email);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> RemoveOrderFromList(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int userId = User.Identity.GetUserId<int>();

            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            OrdersIndexViewModel model = mapper.Map<OrdersIndexViewModel>(order);

            return View(model);
        }

        [HttpPost, ActionName("RemoveOrderFromList")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveOrderFromListConfirm(int orderId = 0)
        {
            await orderService.RemoveOrderFormCustomerOrdersListAsync(orderId);
            TempData["message"] = "Zamówienie zostało usunięte";
            return RedirectToAction("Index");
        }
    }
}