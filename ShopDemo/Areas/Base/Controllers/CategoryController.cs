using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Base.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;
        private readonly ISearchFilterService searchFilterService;
        private readonly IMapper mapper;

        public CategoryController(ICategoryService categoryService, ISearchFilterService searchFilterService, IMapper mapper)
        {
            this.categoryService = categoryService;
            this.searchFilterService = searchFilterService;
            this.mapper = mapper;
        }

        public async Task<PartialViewResult> GetCategoriesMenuPartial(int? categoryId, string targetActionName, string areaName, string filtr)
        {
            CategoryMenuViewModel viewModel = new CategoryMenuViewModel();
            viewModel.ActionName = targetActionName;
            viewModel.AreaName = areaName;

            bool existsAsParent = await categoryService.CategoryExistsAsParentAsync(categoryId);
            IEnumerable<CategoryMenuBaseViewModel> categoriesList;
            if (existsAsParent)
            {
                var categories = await categoryService.GetAllAsync(c => c.ParentCategoryId == categoryId);
                categoriesList = mapper.Map<IEnumerable<CategoryMenuBaseViewModel>>(categories);
                foreach (var category in categoriesList)
                {
                    var filterValues = await searchFilterService.RemoveUnexistingParametesFromFiltrAsync(category.CategoryId, filtr.ToDictionary());
                    category.SearchFilter = filterValues.ToSearchString();
                }
            }
            else
            {
                int? parentId = await categoryService.FindCategoryParentIdAsync(categoryId);
                var categories = await categoryService.GetAllAsync(c => c.ParentCategoryId == parentId);
                categoriesList = mapper.Map<IEnumerable<CategoryMenuBaseViewModel>>(categories);
                foreach (var category in categoriesList)
                {
                    var filterValues = await searchFilterService.RemoveUnexistingParametesFromFiltrAsync(category.CategoryId, filtr.ToDictionary());
                    category.SearchFilter = filterValues.ToSearchString();
                    if (category.CategoryId == categoryId)
                    {
                        category.IsSelected = true;
                    }
                }
            }
            viewModel.Categories = categoriesList;
            return PartialView("_CategoriesMenuPartial", viewModel);
        }

        public async Task<PartialViewResult> RetrivePreviousSelectedCategories(int? categoryId, string targetActionName, string areaName)
        {
            ViewBag.TargetActionName = targetActionName;
            ViewBag.AreaName = areaName;
            List<CategoryDropDownViewModel> previousCategories = null;
            if (categoryId != null)
            {
                IEnumerable<Category> categories = await categoryService.GetAllAsync();
                Category category = categories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category != null)
                {
                    int? parentId = category.ParentCategoryId;

                    previousCategories = new List<CategoryDropDownViewModel>();
                    previousCategories.Add(new CategoryDropDownViewModel { CategoryId = category.CategoryId, Name = category.Name });
                    while (parentId != null)
                    {
                        Category cat = categories.FirstOrDefault(c => c.CategoryId == parentId);
                        previousCategories.Add(new CategoryDropDownViewModel { CategoryId = cat.CategoryId, Name = cat.Name });
                        parentId = cat.ParentCategoryId;
                    }
                    previousCategories.Reverse();
                }
            }
            return PartialView("_PreviousSelectedCategoriesPartial", previousCategories);
        }
    }
}