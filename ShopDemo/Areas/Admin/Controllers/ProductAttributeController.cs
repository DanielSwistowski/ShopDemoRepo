using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductAttributeController : Controller
    {
        private readonly IProductAttributeService productAttributeService;
        private readonly IMapper mapper;

        public ProductAttributeController(IProductAttributeService productAttributeService, IMapper mapper)
        {
            this.productAttributeService = productAttributeService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> GetProductAttributes(int categoryId)
        {
            var attributes = await productAttributeService.GetAllAsync(p => p.CategoryId == categoryId);
            IEnumerable<ProductAttributeViewModel> attributesViewModel = mapper.Map<IEnumerable<ProductAttributeViewModel>>(attributes);
            return PartialView("_ProductAttributesPartial", attributesViewModel);
        }

        public async Task<ActionResult> GetParentCategoryProductAttributesDistinct(int categoryId, int? parentCategoryId)
        {
            if (parentCategoryId == null)
            {
                return PartialView("_ParentCategoryProductAttributesPartial", null);
            }
            else
            {
                var attributes = await productAttributeService.GetAllAsync(p => p.CategoryId == categoryId);
                var parentAttributes = await productAttributeService.GetAllAsync(p => p.CategoryId == parentCategoryId);

                List<ParentCategoryProductAtributeViewModel> attributesViewModel = new List<ParentCategoryProductAtributeViewModel>();

                foreach (var parentAttribute in parentAttributes)
                {
                    if (!attributes.Any(a => a.Name.Equals(parentAttribute.Name)))
                    {
                        attributesViewModel.Add(new ParentCategoryProductAtributeViewModel { Name = parentAttribute.Name, IsSelected = true });
                    }
                }

                return PartialView("_ParentCategoryProductAttributesPartial", attributesViewModel as IEnumerable<ParentCategoryProductAtributeViewModel>);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> AddProductAttributes(AddProductAttributesViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<ProductAttribute> attributesList = new List<ProductAttribute>();
                foreach (var attrib in model.Attributes)
                {
                    ProductAttribute attribute = new ProductAttribute();
                    attribute.CategoryId = model.CategoryId;
                    attribute.Name = attrib;
                    attributesList.Add(attribute);
                }
                await productAttributeService.AddMultipleProductAttributesAsync(attributesList);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> RemoveProductAttribute(int attributeId)
        {
            ProductAttribute productAttribute = await productAttributeService.FindByIdAsync(attributeId);
            if (productAttribute == null)
            {
                return Json(new { success = false, message = "Wybrana cecha produktu nie istnieje" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                await productAttributeService.DeleteAsync(productAttribute);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> EditProductAttribute(EditProductAttributeViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool attributeExists = await productAttributeService.ProductAttributeExistsAsync(model.ProductAttributeId);
                if (!attributeExists)
                {
                    return Json(new { success = false, message = "Wybrana cecha produktu nie istnieje" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ProductAttribute productAttribute = new ProductAttribute();
                    productAttribute.ProductAttributeId = model.ProductAttributeId;
                    productAttribute.Name = model.Name;
                    await productAttributeService.UpdateAsync(productAttribute);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetProductAttributesWithValuesJson(int categoryId)
        {
            string[] includedProperty = { "ProductAttributeValues" };
            var attributesWithValues = await productAttributeService.GetAllAsync(c => c.CategoryId == categoryId, includedProperty);
            IEnumerable<ProductAttributeWithValuesViewModel> attributesWithValuesModel = mapper.Map<IEnumerable<ProductAttributeWithValuesViewModel>>(attributesWithValues);
            return Json(attributesWithValuesModel, JsonRequestBehavior.AllowGet);
        }
    }
}