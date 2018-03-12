using AutoMapper;
using DataAccessLayer.Models;
using DataAccessLayer.Models.Payu;
using Microsoft.AspNet.Identity;
using Service_Layer.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    [Authorize]
    public class PayuController : Controller
    {
        private readonly IPayuService payuService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;
        
        public PayuController(IPayuService payuService, IOrderService orderService, IMapper mapper)
        {
            this.payuService = payuService;
            this.orderService = orderService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> CreateNewOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int userId = User.Identity.GetUserId<int>();

            string[] includedProperties = { "DeliveryOption", "User", "OrderDetails", "OrderDetails.Product" };
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId && o.UserId == userId, includedProperties);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            List<DataAccessLayer.Models.Payu.Product> buyedProducts = mapper.Map<List<DataAccessLayer.Models.Payu.Product>>(order.OrderDetails);

            // add delivery as product
            buyedProducts.Add(new DataAccessLayer.Models.Payu.Product { Name = order.DeliveryOption.Option, Quantity = 1, UnitPrice = (int)(order.DeliveryOption.Price * 100) });

            OrderCreateRequest requestData = new OrderCreateRequest();
            requestData.Buyer = mapper.Map<Buyer>(order.User);
            requestData.CustomerIp = Request.UserHostAddress;
            requestData.Description = "Zamówienie nr " + order.OrderId;
            requestData.ExtOrderId = order.OrderId.ToString();
            requestData.Products = buyedProducts;
            requestData.TotalAmount = (long)(order.TotalAmount * 100);
            requestData.ContinueUrl = Url.Action("PaymentComplete", "Payu", new { orderId = orderId }, Request.Url.Scheme);
            requestData.NotifyUrl = Url.Action("PayuPaymentStatusNotify", "Payu", new { area = "Admin" }, Request.Url.Scheme);

            OrderCreateResponse response = await payuService.CreateNewOrderAsync(requestData);

            return Redirect(response.RedirectUri);
        }

        public async Task<ActionResult> MakePaymentForAnExistingOrder(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int userId = User.Identity.GetUserId<int>();

            string[] includedProperties = { "DeliveryOption", "User", "OrderDetails", "OrderDetails.Product" };
            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId && o.UserId == userId, includedProperties);

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            List<DataAccessLayer.Models.Payu.Product> buyedProducts = mapper.Map<List<DataAccessLayer.Models.Payu.Product>>(order.OrderDetails);

            // add delivery as product
            buyedProducts.Add(new DataAccessLayer.Models.Payu.Product { Name = order.DeliveryOption.Option, Quantity = 1, UnitPrice = (int)(order.DeliveryOption.Price * 100) });

            OrderCreateRequest requestData = new OrderCreateRequest();
            requestData.Buyer = mapper.Map<Buyer>(order.User);
            requestData.CustomerIp = Request.UserHostAddress;
            requestData.Products = buyedProducts;
            requestData.TotalAmount = (long)(order.TotalAmount * 100);
            int newOrderId = await orderService.CreateNewOrderIdAsync(order);// PayU requires unique extOrderId
            requestData.ExtOrderId = newOrderId.ToString();
            requestData.Description = "Płatność za zamówienie nr " + orderId;
            requestData.ContinueUrl = Url.Action("PaymentComplete", "Payu", new { orderId = newOrderId }, Request.Url.Scheme);
            requestData.NotifyUrl = Url.Action("PayuPaymentStatusNotify", "Payu", new { area = "Admin" }, Request.Url.Scheme);

            OrderCreateResponse response = await payuService.CreateNewOrderAsync(requestData);

            return Redirect(response.RedirectUri);
        }

        public async Task<ActionResult> PaymentComplete(int orderId, string error = "")
        {
            ViewBag.OrderId = orderId;

            if (error == "501")
            {
                return View("PaymentError");
            }

            await orderService.ChangeStatusAsync(orderId, OrderStatus.WaitingForPaymentComplete);

            return View();
        }
    }
}