using AutoMapper;
using Microsoft.AspNet.Identity;
using Service_Layer.Services;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    [Authorize]
    public class DeliveryController : Controller
    {
        private readonly IDeliveryService deliveryService;
        private readonly IMapper mapper;
        private ApplicationUserManager userManager;
        public DeliveryController(IDeliveryService deliveryService, IMapper mapper, ApplicationUserManager userManager)
        {
            this.deliveryService = deliveryService;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public async Task<ActionResult> SelectDeliveryOption()
        {
            int userId = User.Identity.GetUserId<int>();

            bool userAddressIsProvided = await userManager.IsUserAddressSetAsync(userId);

            if (!userAddressIsProvided)
                return RedirectToAction("AddAddress", "Manage");

            var deliveryOptions = await deliveryService.GetAllAsync(d => d.IsActive == true);
            IEnumerable<DeliveryOptionsViewModel> deliveryOptionsModel = mapper.Map<IEnumerable<DeliveryOptionsViewModel>>(deliveryOptions);
            return View(deliveryOptionsModel);
        }
    }
}