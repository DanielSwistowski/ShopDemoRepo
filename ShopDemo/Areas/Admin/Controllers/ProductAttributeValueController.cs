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
    public class ProductAttributeValueController : Controller
    {
        private readonly IProductAttributeValueService productAttributeValueService;
        private readonly IMapper mapper;

        public ProductAttributeValueController(IProductAttributeValueService productAttributeValueService, IMapper mapper)
        {
            this.productAttributeValueService = productAttributeValueService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> GetProductAttributeValues(int attributeId)
        {
            var attributeValues = await productAttributeValueService.GetAllAsync(p => p.ProductAttributeId == attributeId);
            IEnumerable<ProductAttributeValueViewModel> attributesViewModel = mapper.Map<IEnumerable<ProductAttributeValueViewModel>>(attributeValues);
            return PartialView("_ProductAttributeValuesPartial", attributesViewModel);
        }

        public async Task<ActionResult> GetParentProductAttributeValuesDistinct(int? categoryId, string attributeName, int attributeId)
        {
            List<ParentAttributeValueViewModel> attributeValuesDistinct = new List<ParentAttributeValueViewModel>();
            if (categoryId != null)
            {
                var attributeValues = await productAttributeValueService.GetAllAsync(a => a.ProductAttributeId == attributeId);
                var parentAttributeValues = await productAttributeValueService.GetProductAttributeValuesByCategoryIdAndAttributeNameAsync((int)categoryId, attributeName);

                foreach (var parentAttributeValue in parentAttributeValues)
                {
                    if (!attributeValues.Any(a => a.AttributeValue.Equals(parentAttributeValue.AttributeValue)))
                    {
                        attributeValuesDistinct.Add(new ParentAttributeValueViewModel { AttributeValue = parentAttributeValue.AttributeValue, IsSelected = true });
                    }
                }
            }

            return PartialView("_ParentProductAttributeValuesPartial", attributeValuesDistinct);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> AddProductAttributeValue(AddProductAttributeValueViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();
                foreach (var value in model.AttributeValues)
                {
                    attributeValues.Add(new ProductAttributeValue { AttributeValue = value, ProductAttributeId = model.ProductAttributeId });
                }
                await productAttributeValueService.AddMultipleProductAttributeValuesAsync(attributeValues);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> RemoveProductAttributeValue(int attributeValueId)
        {
            ProductAttributeValue attributeValue = await productAttributeValueService.FindByIdAsync(attributeValueId);
            if (attributeValue == null)
            {
                return Json(new { success = false, message = "Wybrana wartość cechy produktu nie istnieje." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                await productAttributeValueService.DeleteAsync(attributeValue);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}