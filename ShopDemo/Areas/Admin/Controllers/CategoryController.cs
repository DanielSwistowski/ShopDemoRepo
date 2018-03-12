using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using ShopDemo.CustomValidationAttributes;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            this.categoryService = categoryService;
            this.mapper = mapper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetCategoriesJson(int? categoryId)
        {
            var categories = await categoryService.GetAllAsync(c => c.ParentCategoryId == categoryId);
            IEnumerable<CategoryViewModel> categoriesList = mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return Json(categoriesList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> AddCategory(AddCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                Category category = mapper.Map<Category>(model);
                await categoryService.AddAsync(category);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> EditCategory(EditCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                Category category = await categoryService.FindByIdAsync(model.CategoryId);
                if (category == null)
                {
                    return Json(new { success = false, message = "Wybrana kategoria nie istnieje" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    category.Name = model.Name;
                    await categoryService.UpdateAsync(category);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Uzupełnij wszystkie dane" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> DeleteCategory(int categoryId)
        {
            bool categoryExistsAsParentCategory = await categoryService.CategoryExistsAsParentAsync(categoryId);

            if (categoryExistsAsParentCategory)
                return Json(new { success = false, message = "Nie można usunąc wybranej kategorii, ponieważ zawiera ona podkategorie" }, JsonRequestBehavior.AllowGet);

            bool categoryContainsProducts = await categoryService.CategoryContainsProductsAsync(categoryId);

            if (categoryContainsProducts)
                return Json(new { success = false, message = "Nie można usunąc wybranej kategorii, ponieważ istnieją przypisane do niej produkty" }, JsonRequestBehavior.AllowGet);

            Category category = await categoryService.FindByIdAsync(categoryId);

            if (category == null)
                return Json(new { success = false, message = "Wybrana kategoria nie istnieje" }, JsonRequestBehavior.AllowGet);

            await categoryService.DeleteAsync(category);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetCategoriesForDropDownJson(int? categoryId)
        {
            var categories = await categoryService.GetAllAsync(c => c.ParentCategoryId == categoryId);
            IEnumerable<CategoryDropDownViewModel> categoryDropDownModel = mapper.Map<IEnumerable<CategoryDropDownViewModel>>(categories);
            return Json(categoryDropDownModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CategoryExistsAsParentJson(int? categoryId)
        {
            bool exists = await categoryService.CategoryExistsAsParentAsync(categoryId);
            return Json(exists, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ResolveSelectedCategoriesForDropDown(IEnumerable<int> selectedCategories)
        {
            if (selectedCategories != null)
            {
                List<SelectedCategoryViewModel> categories = await ResolveSelectedCategories(selectedCategories);
                return Json(categories, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> FindProductCategoriesByProductName(string productName)
        {
            IEnumerable<int> categoriesIds = await categoryService.FindProductCategoriesIdsByProductNameAsync(productName);
            if (categoriesIds != null)
            {
                if (categoriesIds.Count() > 0)
                {
                    List<SelectedCategoryViewModel> categories = await ResolveSelectedCategories(categoriesIds);
                    return Json(new { data = categories }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<SelectedCategoryViewModel>> ResolveSelectedCategories(IEnumerable<int> selectedCategories)
        {
            List<SelectedCategoryViewModel> selectedCategoriesList = new List<SelectedCategoryViewModel>();
            int? parentCategoryId = null;
            foreach (var selectedCategoryId in selectedCategories)
            {
                var categories = await categoryService.GetAllAsync(c => c.ParentCategoryId == parentCategoryId);
                List<CategoryDropDownViewModel> categoryDropDownViewModel = new List<CategoryDropDownViewModel>();
                foreach (var category in categories)
                {
                    categoryDropDownViewModel.Add(new CategoryDropDownViewModel { CategoryId = category.CategoryId, Name = category.Name });
                }
                selectedCategoriesList.Add(new SelectedCategoryViewModel { Categories = categoryDropDownViewModel, SelectedCategoryId = selectedCategoryId });
                parentCategoryId = selectedCategoryId;
            }
            return selectedCategoriesList;
        }
    }
}