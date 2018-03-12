using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeliveryController : Controller
    {
        private readonly IDeliveryService deliveryService;
        private readonly IMapper mapper;

        public DeliveryController(IDeliveryService deliveryService, IMapper mapper)
        {
            this.deliveryService = deliveryService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> Index()
        {
            var deliveryOptions = await deliveryService.GetAllAsync();

            IEnumerable<AdminDeliveryOptionsViewModel> deliveryOptionsModel = mapper.Map<IEnumerable<AdminDeliveryOptionsViewModel>>(deliveryOptions);

            return View(deliveryOptionsModel);
        }

        public ActionResult AddNewDelivery()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewDelivery(CrudDeliveryViewModel model)
        {
            if (ModelState.IsValid)
            {
                Delivery delivery = mapper.Map<Delivery>(model);

                await deliveryService.AddAsync(delivery);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<ActionResult> DeleteDelivery(int deliveryId = 0)
        {
            if (deliveryId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);

            if (delivery == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                        
            var deliveryModel = mapper.Map<CrudDeliveryViewModel>(delivery);

            if (TempData["errorMessage"] != null)
                ViewBag.ErrorMessage = (string)TempData["errorMessage"];

            return View(deliveryModel);
        }

        [HttpPost,ActionName("DeleteDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDeliveryConfirm(int deliveryId)
        {
            bool deliveryExistsInOrders = await deliveryService.DeliveryExistsInOrdersAsync(deliveryId);
            if (deliveryExistsInOrders)
            {
                Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);
                if (delivery.IsActive)
                {
                    TempData["errorMessage"] = "Nie możesz usunąć wybranego sposobu dostawy, ponieważ istnieje on jeszcze na liście zamówień. "
                    + "Jeśli chcesz, aby wybrany sposób był niedostępny możesz go deaktywować.";
                    return RedirectToAction("DeactivateDeliveryOption", new { deliveryId = deliveryId });
                }
                else
                {
                    TempData["errorMessage"] = "Nie możesz usunąć wybranego sposobu dostawy, ponieważ istnieje on jeszcze na liście zamówień.";
                    return RedirectToAction("DeleteDelivery", new { deliveryId = deliveryId });
                }
            }

            await deliveryService.DeleteAsync(deliveryId);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeactivateDeliveryOption(int deliveryId = 0)
        {
            if (deliveryId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);

            if (delivery == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var deliveryModel = mapper.Map<CrudDeliveryViewModel>(delivery);

            if (TempData["errorMessage"] != null)
                ViewBag.ErrorMessage = (string)TempData["errorMessage"];

            return View(deliveryModel);
        }

        [HttpPost,ActionName("DeactivateDeliveryOption")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeactivateDeliveryOptionConfirm(int deliveryId)
        {
            await deliveryService.DeactivateDeliveryOptionAsync(deliveryId);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ActivateDeliveryOption(int deliveryId = 0)
        {
            if (deliveryId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Delivery delivery = await deliveryService.FindByIdAsync(deliveryId);

            if (delivery == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var deliveryModel = mapper.Map<CrudDeliveryViewModel>(delivery);

            return View(deliveryModel);
        }

        [HttpPost, ActionName("ActivateDeliveryOption")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateDeliveryOptionConfirm(int deliveryId)
        {
            await deliveryService.ActivateDeliveryOptionAsync(deliveryId);
            return RedirectToAction("Index");
        }
    }
}