using AutoMapper;
using DataAccessLayer.Models;
using Hangfire;
using PagedList;
using Postal;
using Rotativa.MVC;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService orderService;
        private readonly IDeliveryService deliveryService;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;
        private readonly IBackgroundJobClient jobClient;
        private int PageSize = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TablePageSize"]);

        public OrderController(IOrderService orderService, IDeliveryService deliveryService, IEmailService emailService, IMapper mapper, IBackgroundJobClient jobClient)
        {
            this.orderService = orderService;
            this.deliveryService = deliveryService;
            this.emailService = emailService;
            this.mapper = mapper;
            this.jobClient = jobClient;
        }

        public ActionResult Index()
        {
            if (TempData["message"] != null)
                ViewBag.Message = (string)TempData["message"];

            return View();
        }

        public async Task<ActionResult> GetAllOrders(int? page, int orderId = 0, OrderStatus orderStatus = OrderStatus.Uncompleted)
        {
            int pageNumber = (page ?? 1);
            ViewBag.OrderId = orderId;
            ViewBag.OrderStatus = orderStatus;

            Expression<Func<Order, bool>> filter = o => o.OrderStatus == orderStatus;

            if (orderId != 0)
                filter = o => o.OrderStatus == orderStatus && o.OrderId.ToString().Contains(orderId.ToString());

            var orders = await orderService.PageAllAsync(filter, s => s.OrderBy(o => o.OrderDate), null, pageNumber, PageSize);
            int ordersCount = await orderService.EntitiesCountAsync(filter);

            IEnumerable<OrdersIndexViewModel> ordersModel = mapper.Map<IEnumerable<OrdersIndexViewModel>>(orders);

            var pagedList = new StaticPagedList<OrdersIndexViewModel>(ordersModel, pageNumber, PageSize, ordersCount);

            return PartialView("_OrdersListPartial", pagedList);
        }

        public async Task<ActionResult> Details(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperties = { "DeliveryOption", "User", "User.Address", "OrderDetails", "OrderDetails.Product" };
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, includedProperties);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            OrderDetailViewModel model = mapper.Map<OrderDetailViewModel>(order);
            model.OrderBaseData = mapper.Map<OrdersIndexViewModel>(order);

            if (TempData["ErrorMessage"] != null)
                ViewBag.ErrorMessage = (string)TempData["ErrorMessage"];

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = (string)TempData["SuccessMessage"];

            return View(model);
        }

        public async Task<ActionResult> RealizeOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, new string[] { "DeliveryOption" });

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (order.OrderStatus == OrderStatus.DuringRealization)
            {
                TempData["message"] = "Wybrane zamówienie jest aktualnie realizowane.";
                return RedirectToAction("Index");
            }

            RealizeOrderViewModel model = mapper.Map<RealizeOrderViewModel>(order);
            model.OrderBaseData = mapper.Map<OrdersIndexViewModel>(order);

            await orderService.ChangeStatusAsync(orderId, OrderStatus.DuringRealization);
            string realizeOrderJobId = jobClient.Schedule(() => orderService.ChangeStatusToUncompleted(orderId), TimeSpan.FromMinutes(15));
            TempData["realizeOrderJobId"] = realizeOrderJobId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public JsonResult EnableOrderToRealization(int orderId)
        {
            orderService.ChangeStatusToUncompleted(orderId);

            string realizeOrderJobId;
            if (TempData["realizeOrderJobId"] != null)
            {
                realizeOrderJobId = (string)TempData["realizeOrderJobId"];
                jobClient.Delete(realizeOrderJobId);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("RealizeOrder")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RealizeOrderConfirm(RealizeOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                await orderService.ChangeStatusAsync(model.OrderBaseData.OrderId, OrderStatus.Completed);
                string emailAddress = await orderService.FindUserEmailByOrderId(model.OrderBaseData.OrderId);
                string orderDetailsUrl = Url.Action("Details", "Order", new { orderId = model.OrderBaseData.OrderId, area = "" }, Request.Url.Scheme);
                var email = new OrderInfoEmail
                {
                    To = emailAddress,
                    Subject = EncodeStringHelpers.ConvertStringToUtf8("Zamówienie zrealizowane"),
                    Message = "Twoje zamówienie zostało zrealizowane i przekazane do wysyłki. Kliknij w poniższy link, aby przejść na stronę sklepu i zobaczyć szczegóły zamówienia.",
                    OrderDetailsUrl = orderDetailsUrl
                };
                await emailService.SendAsync(email);

                string realizeOrderJobId;
                if (TempData["realizeOrderJobId"] != null)
                {
                    realizeOrderJobId = (string)TempData["realizeOrderJobId"];
                    jobClient.Delete(realizeOrderJobId);
                }

                return RedirectToAction("RealizeOrderSuccess", new { orderId = model.OrderBaseData.OrderId });
            }
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == model.OrderBaseData.OrderId, new string[] { "DeliveryOption" });
            model = mapper.Map<RealizeOrderViewModel>(order);
            model.OrderBaseData = mapper.Map<OrdersIndexViewModel>(order);
            return View(model);
        }

        public ActionResult RealizeOrderSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        public async Task<ActionResult> PrintBill(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, new string[] { "DeliveryOption" });

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            OrderBillViewModel model = mapper.Map<OrderBillViewModel>(order);
            model.ShopName = System.Configuration.ConfigurationManager.AppSettings["ShopName"];
            model.ShopStreet = System.Configuration.ConfigurationManager.AppSettings["ShopAddressStreet"];
            model.ShopCity = System.Configuration.ConfigurationManager.AppSettings["ShopAddressZipCode"];
            model.PaymentOption = order.DeliveryOption.PaymentOption;

            return new ViewAsPdf("OrderBill", model);
        }

        public async Task<ActionResult> CancelOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, new string[] { "DeliveryOption" });

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (order.OrderStatus == OrderStatus.CancelledByAdmin)
            {
                TempData["ErrorMessage"] = "Wybrane zamówienie zostało już anulowane.";
                return RedirectToAction("Details", new { orderId = orderId });
            }

            AdminCancelOrder model = mapper.Map<AdminCancelOrder>(order);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelOrder(AdminCancelOrder model)
        {
            if (ModelState.IsValid)
            {
                await orderService.CancelOrderAsync(model.OrderId, OrderStatus.CancelledByAdmin);

                if (model.PaymentOption == PaymentOptions.PaymentByTransfer && model.OrderStatus == OrderStatusViewModel.Uncompleted)
                {
                    TempData["CancelOrderReason"] = model.Message;
                    return RedirectToAction("Refund", "Payu", new { orderId = model.OrderId });
                }
                else
                {
                    string emailAddress = await orderService.FindUserEmailByOrderId(model.OrderId);
                    string orderDetailsUrl = Url.Action("Details", "Order", new { orderId = model.OrderId, area = "" }, Request.Url.Scheme);
                    var email = new OrderInfoEmail
                    {
                        To = emailAddress,
                        Subject = EncodeStringHelpers.ConvertStringToUtf8("Zamówienie anulowane"),
                        Message = "Twoje zamówienie zostało anulowane. Powód: " + model.Message + ". Kliknij w poniższy link, aby przejść na stronę sklepu i zobaczyć szczegóły anulowanego zamówienia.",
                        OrderDetailsUrl = orderDetailsUrl
                    };
                    await emailService.SendAsync(email);
                    TempData["SuccessMessage"] = "Zamówienie zostało zanulowane";
                    return RedirectToAction("Details", new { orderId = model.OrderId });
                }
            }
            return View(model);
        }

        public async Task<ActionResult> CancelOrderSuccess(int orderId)
        {
            string cancelOrderReason = "Brak danych";

            if (TempData["CancelOrderReason"] != null)
                cancelOrderReason = (string)TempData["CancelOrderReason"];

            string emailAddress = await orderService.FindUserEmailByOrderId(orderId);
            string orderDetailsUrl = Url.Action("Details", "Order", new { orderId = orderId, area = "" }, Request.Url.Scheme);
            var email = new OrderInfoEmail
            {
                To = emailAddress,
                Subject = EncodeStringHelpers.ConvertStringToUtf8("Zamówienie anulowane"),
                Message = "Twoje zamówienie zostało anulowane. Powód: " + cancelOrderReason + ". Proszę oczekiwać na zwrot środków na konto. Kliknij w poniższy link, aby przejść na stronę sklepu i zobaczyć szczegóły anulowanego zamówienia.",
                OrderDetailsUrl = orderDetailsUrl
            };
            await emailService.SendAsync(email);
            TempData["SuccessMessage"] = "Zamówienie zostało zanulowane";
            return RedirectToAction("Details", new { orderId = orderId });
        }

        public async Task<ActionResult> UserOrders(string userEmail, int userId = 0)
        {
            if (userId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.UserEmail = userEmail.FromBase64();
            ViewBag.UserId = userId;

            IEnumerable<Order> userOrders = await orderService.GetAllAsync(u => u.UserId == userId);

            IEnumerable<UserOrdersListViewModel> ordersViewModel = userOrders.GroupBy(o => o.OrderStatus)
                .Select(g => new UserOrdersListViewModel { OrderStatus = (OrderStatusViewModel)g.Key, UserOrders = mapper.Map<List<UserOrderViewModel>>(g) });

            return View(ordersViewModel);
        }

        public async Task<ActionResult> GetOrdersForSelectedMonth(string monthName, int year, [Bind(Prefix = "strona")]int? page)
        {
            int pageNumber = page ?? 1;
            ViewBag.MonthName = monthName;
            ViewBag.Year = year;

            int monthNumber = DateHelper.GetMonthNumber(monthName);
            var firstDayOfMonth = new DateTime(year, monthNumber, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

            Expression<Func<Order, bool>> filter = o => o.OrderRealizationDate >= firstDayOfMonth && o.OrderRealizationDate <= lastDayOfMonth;

            var orders = await orderService.PageAllAsync(filter, s => s.OrderBy(o => o.OrderDate), null, pageNumber, PageSize);
            int ordersCount = await orderService.EntitiesCountAsync(filter);

            IEnumerable<OrdersIndexViewModel> ordersModel = mapper.Map<IEnumerable<OrdersIndexViewModel>>(orders);

            var pagedList = new StaticPagedList<OrdersIndexViewModel>(ordersModel, pageNumber, PageSize, ordersCount);

            return View(pagedList);
        }
    }
}