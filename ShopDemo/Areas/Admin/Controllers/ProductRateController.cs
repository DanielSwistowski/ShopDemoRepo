using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductRateController : Controller
    {
        private readonly IProductRateService productRateService;
        private readonly IMapper mapper;

        public ProductRateController(IProductRateService productRateService, IMapper mapper)
        {
            this.productRateService = productRateService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> GetAllProductComments(int productId)
        {
            var comments = await productRateService.GetAllAsync(p => p.ProductId == productId);

            if (comments.Count() > 0)
            {
                ProductRateListViewModel model = new ProductRateListViewModel();
                model.Comments = mapper.Map<List<ProductRateViewModel>>(comments);
                model.TotalRate = comments.Average(r => r.Rate);

                return PartialView("_ProductCommentsPartial", model);
            }
            else
            {
                return PartialView("_ProductCommentsNotExistsPartial");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> DeleteComment(int productRateId)
        {
            ProductRate rate = await productRateService.FindByIdAsync(productRateId);
            
            if(rate == null)
            {
                return Json(new { success = false, message = "Wybrany komentarz nie istnieje" }, JsonRequestBehavior.AllowGet);
            }

            await productRateService.DeleteAsync(rate);

            return Json(new { success = true, message = "Komentarz został usunięty" }, JsonRequestBehavior.AllowGet);
        }
    }
}