using AutoMapper;
using DataAccessLayer.Models;
using DataAccessLayer.Models.Payu;
using Service_Layer.Services;
using ShopDemo.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
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
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> PayuPaymentStatusNotify(DataAccessLayer.Models.Payu.Order order)
        {
            Request.InputStream.Position = 0;
            string input = await new StreamReader(Request.InputStream).ReadToEndAsync();
            
            string payuHeader = Request.Headers["OpenPayu-Signature"];

            bool signatureIsValid = payuService.PayuSignatureIsValid(input, payuHeader);

            if (!signatureIsValid)
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            
            int orderId;
            bool result = int.TryParse(order.ExtOrderId, out orderId);
            if (result)
            {
                var existingOrder = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, new string[] { "PayuData" });
                if (existingOrder != null)
                {
                    if (existingOrder.PayuData != null)
                    {
                        if (existingOrder.PayuData.PayuOrderId == order.OrderId)
                        {
                            if (existingOrder.PayuData.PaymentStatus == "COMPLETED")
                                return new HttpStatusCodeResult(HttpStatusCode.OK);

                            if (existingOrder.PayuData.PaymentStatus == "CANCELED" && order.Status == "REJECTED")
                            {
                                await orderService.SavePaymentAsync(orderId, order.Status, order.OrderId);
                                return new HttpStatusCodeResult(HttpStatusCode.OK);
                            }
                        }
                    }
                }

                if (order.Status == "COMPLETED" || order.Status == "CANCELED")
                    await orderService.SavePaymentAsync(orderId, order.Status, order.OrderId);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        public async Task<ActionResult> Refund(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string payuOrderId = await orderService.GetPayuOrderIdAsync(orderId);

            RefundOrderResponse response = null;
            try
            {
                response = await payuService.RefundAsync(payuOrderId);

                if (response.status.statusCode != "SUCCESS")
                {
                    await orderService.ChangeStatusAsync(orderId, OrderStatus.Uncompleted); //set previous order status
                    TempData["ErrorMessage"] = "Nie można anulować zamówienia i  dokonać zwrotu płatności. Komunikat z Payu: " + GetPayuStatusDescription(response.status.code);
                    return RedirectToAction("Details", "Order", new { orderId = orderId });
                }
                else
                {
                    return RedirectToAction("CancelOrderSuccess", "Order", new { orderId = orderId });
                }
            }
            catch (Exception ex)
            {
                await orderService.ChangeStatusAsync(orderId, OrderStatus.Uncompleted); //set previous order status
                TempData["ErrorMessage"] = "Nie można anulować zlecenia. Szczegóły błędu: " + ex.Message;
                return RedirectToAction("Details", "Order", new { orderId = orderId });
            }
        }

        public async Task<ActionResult> AcceptPayment(int orderId = 0)
        {
            if (orderId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await orderService.FindByPredicateAsync(o => o.OrderId == orderId, new string[] { "DeliveryOption" });

            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (order.OrderStatus != OrderStatus.PaymentRejected)
            {
                TempData["ErrorMessage"] = "Płatność jest już zaakceptowana.";
                return RedirectToAction("Details", "Order", new { orderId = orderId });
            }

            AdminCancelOrder model = mapper.Map<AdminCancelOrder>(order);

            return View(model);
        }

        [HttpPost, ActionName("AcceptPayment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AcceptPaymentConfirm(int orderId)
        {
            string payuOrderId = await orderService.GetPayuOrderIdAsync(orderId);

            AcceptPayment responseStatus = await payuService.AcceptPaymentAsync(payuOrderId);

            if (responseStatus.status.statusCode == "SUCCESS")
            {
                await orderService.SavePaymentAsync(orderId, "COMPLETED", payuOrderId);
                TempData["SuccessMessage"] = "Płatność została zaakceptowana.";
                return RedirectToAction("Details", "Order", new { orderId = orderId });
            }
            else
            {
                TempData["ErrorMessage"] = "Błąd! Nie można zaakceptować płatności. Kompunikat Payu: " + responseStatus.status.statusDesc;
                return RedirectToAction("Details", "Order", new { orderId = orderId });
            }
        }


        //helpers
        private string GetPayuStatusDescription(string code)
        {
            string description = "";
            switch (code)
            {
                case "9101":
                    description = "Transakcja nie jest zakończona.";
                    break;
                case "9102":
                    description = "Brak środków na koncie do zwrotu.";
                    break;
                case "9103":
                    description = "Za duża wartość.";
                    break;
                case "9104":
                    description = "Za mała wartość.";
                    break;
                case "9105":
                    description = "Zwroty są nieaktywne.";
                    break;
                case "9106":
                    description = "Za częsty zwrot.";
                    break;
                case "9108":
                    description = "Zwrot już został wykonany.";
                    break;
                case "9111":
                    description = "Nieznany błąd.";
                    break;
                case "9112":
                    description = "Błędne parametry żądania";
                    break;
                case "9113":
                    description = "Billing sklepu nie jest jeszcze kompletny.";
                    break;
                default:
                    description = "Nieznany błąd.";
                    break;
            }
            return description;
        }
    }
}