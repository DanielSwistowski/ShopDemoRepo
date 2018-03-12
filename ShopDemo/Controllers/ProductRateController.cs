using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    public class ProductRateController : AsyncController
    {
        private readonly IProductRateService productRateService;
        private readonly IMapper mapper;

        public ProductRateController(IProductRateService productRateService, IMapper mapper)
        {
            this.productRateService = productRateService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> GetAllProductComments(int productId = 0)
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
        public async Task<ActionResult> AddComment(AddCommentViewModel model)
        {
            ProductRate rate = mapper.Map<ProductRate>(model);
            rate.CreatedAt = DateTime.Now;

            await productRateService.AddAsync(rate);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> NickNameIsAvailableForCurrentProduct(int productId, string nickName)
        {
            bool isAvailable = await productRateService.NickNameIsAvailableForCurrentProduct(productId, nickName);
            return Json(isAvailable, JsonRequestBehavior.AllowGet);
        }
    }
}