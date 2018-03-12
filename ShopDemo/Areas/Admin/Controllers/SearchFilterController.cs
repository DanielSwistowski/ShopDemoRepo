using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SearchFilterController : Controller
    {
        private readonly ISearchFilterService searchFilterService;
        private readonly IProductAttributeService productAttributeService;
        private readonly IMapper mapper;

        public SearchFilterController(ISearchFilterService searchFilterService, IProductAttributeService productAttributeService, IMapper mapper)
        {
            this.searchFilterService = searchFilterService;
            this.productAttributeService = productAttributeService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> GetSearchFilters(int categoryId)
        {
            string[] includedProperty = { "ProductAttribute" };
            var searchFilters = await searchFilterService.GetAllAsync(c=>c.CategoryId == categoryId, includedProperty);
            IEnumerable<SearchFilterViewModel> searchFiltersModel = mapper.Map<IEnumerable<SearchFilterViewModel>>(searchFilters);
            
            var productAttributes = await productAttributeService.GetAllAsync(c => c.CategoryId == categoryId && c.ProductAttributeValues.Count != 0);
            List<SelectListItem> productAttributesSelectList = new List<SelectListItem>();
            foreach (var attribute in productAttributes)
            {
                productAttributesSelectList.Add(new SelectListItem { Text = attribute.Name, Value = attribute.ProductAttributeId.ToString() });
            }

            AddSearchFilterViewModel addSearchFilterViewModel = new AddSearchFilterViewModel();
            addSearchFilterViewModel.AttributesList = productAttributesSelectList;

            SearchFilterListAndAddFilterViewModel model = new SearchFilterListAndAddFilterViewModel();
            model.AddSearchFilterModel = addSearchFilterViewModel;
            model.SearchFiltersList = searchFiltersModel;

            return PartialView("_ProductSearchFiltersManagementPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> AddSearchFilter(AddSearchFilterViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool searchFilterExists = await searchFilterService.SearchFilterExistsAsync(model.ProductAttributeId);
                if (searchFilterExists)
                {
                    return Json(new { success = false, message = "Filtr już istnieje" }, JsonRequestBehavior.AllowGet);
                }

                SearchFilter filter = mapper.Map<SearchFilter>(model);
                await searchFilterService.AddAsync(filter);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> DeleteSearchFilter(int searchFilterId)
        {
            SearchFilter searchFilter = await searchFilterService.FindByIdAsync(searchFilterId);
            if (searchFilter != null)
            {
                await searchFilterService.DeleteAsync(searchFilter);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Błąd! Wybrany filtr wyszukiwania nie istnieje" }, JsonRequestBehavior.AllowGet);
        }
    }
}