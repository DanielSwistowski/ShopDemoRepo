using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Base.Controllers;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Base.Controllers
{
    [TestFixture]
    public class CategoryControllerTest
    {
        Mock<ICategoryService> mockCategoryService;
        Mock<ISearchFilterService> mockSearchFilterService;
        IMapper mapper;
        List<Category> categoriesList;

        [SetUp]
        public void SetUp()
        {
            mockCategoryService = new Mock<ICategoryService>();
            mockSearchFilterService = new Mock<ISearchFilterService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryProfile());

            });
            mapper = config.CreateMapper();

            categoriesList = new List<Category>();
            categoriesList.Add(new Category { CategoryId = 1, Name = "Category1", ParentCategoryId = null });
            categoriesList.Add(new Category { CategoryId = 2, Name = "Category2", ParentCategoryId = null });
            categoriesList.Add(new Category { CategoryId = 3, Name = "Category3", ParentCategoryId = 1 });
            categoriesList.Add(new Category { CategoryId = 4, Name = "Category4", ParentCategoryId = 1 });
            categoriesList.Add(new Category { CategoryId = 5, Name = "Category5", ParentCategoryId = 1 });
        }

        #region GetCategoriesMenuPartial
        [Test]
        public async Task GetCategoriesMenuPartial_returns_partial_view_CategoriesMenuPartial_with_categories_model_if_category_not_exists_as_parent_category()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int?>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync(categoriesList.Where(p => p.ParentCategoryId == 1));
            mockSearchFilterService.Setup(m => m.RemoveUnexistingParametesFromFiltrAsync(It.IsAny<int>(), It.IsAny<Dictionary<string, IEnumerable<string>>>())).ReturnsAsync(new Dictionary<string, IEnumerable<string>>());

            CategoryController controller = new CategoryController(mockCategoryService.Object, mockSearchFilterService.Object, mapper);

            var result = await controller.GetCategoriesMenuPartial(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) as PartialViewResult;
            var resultModel = (CategoryMenuViewModel)result.Model;

            CategoryMenuBaseViewModel[] categories = resultModel.Categories.ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, resultModel.Categories.Count());
            Assert.AreEqual("Category3", categories[0].Name);
            Assert.AreEqual("Category4", categories[1].Name);
            Assert.AreEqual("Category5", categories[2].Name);
            Assert.AreEqual("_CategoriesMenuPartial", result.ViewName);
        }

        [Test]
        public async Task GetCategoriesMenuPartial_returns_partial_view_CategoriesMenuPartial_with_categories_model_selected_by_parentCategoryId_if_category_exists_as_parent_category()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int?>())).ReturnsAsync(true);
            int? parentCategoryId = null;
            mockCategoryService.Setup(m => m.FindCategoryParentIdAsync(It.IsAny<int>())).ReturnsAsync(parentCategoryId);
            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync(categoriesList.Where(p => p.ParentCategoryId == parentCategoryId));
            mockSearchFilterService.Setup(m => m.RemoveUnexistingParametesFromFiltrAsync(It.IsAny<int>(), It.IsAny<Dictionary<string, IEnumerable<string>>>())).ReturnsAsync(new Dictionary<string, IEnumerable<string>>());

            CategoryController controller = new CategoryController(mockCategoryService.Object, mockSearchFilterService.Object, mapper);

            var result = await controller.GetCategoriesMenuPartial(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) as PartialViewResult;
            var resultModel = (CategoryMenuViewModel)result.Model;

            CategoryMenuBaseViewModel[] categories = resultModel.Categories.ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, resultModel.Categories.Count());
            Assert.AreEqual("Category1", categories[0].Name);
            Assert.AreEqual("Category2", categories[1].Name);
            Assert.AreEqual("_CategoriesMenuPartial", result.ViewName);
        }

        [Test]
        public async Task GetCategoriesMenuPartial_pass_area_name_and_action_name_to_view_model()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int?>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync(new List<Category>());
            mockSearchFilterService.Setup(m => m.RemoveUnexistingParametesFromFiltrAsync(It.IsAny<int>(), It.IsAny<Dictionary<string, IEnumerable<string>>>())).ReturnsAsync(new Dictionary<string, IEnumerable<string>>());

            CategoryController controller = new CategoryController(mockCategoryService.Object, mockSearchFilterService.Object, mapper);

            string actionName = "testActionName";
            string areaName = "testAreaName";
            string filtr = "testFiltr";


            var result = await controller.GetCategoriesMenuPartial(It.IsAny<int?>(), actionName, areaName, filtr) as PartialViewResult;
            var resultModel = (CategoryMenuViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(actionName, resultModel.ActionName);
            Assert.AreEqual(areaName, resultModel.AreaName);
        }
        #endregion


        #region RetrivePreviousSelectedCategories
        [Test]
        public async Task RetrivePreviousSelectedCategories_returns_partial_view_PreviousSelectedCategoriesPartial_with_categories_collection_if_categoryId_is_not_equal_to_null()
        {
            List<Category> categories = new List<Category>();
            categories.Add(new Category { CategoryId = 1, Name = "Category1", ParentCategoryId = null });
            categories.Add(new Category { CategoryId = 2, Name = "Category2", ParentCategoryId = 1 });
            categories.Add(new Category { CategoryId = 3, Name = "Category3", ParentCategoryId = 2 });
            categories.Add(new Category { CategoryId = 4, Name = "Category4", ParentCategoryId = 3 });
            categories.Add(new Category { CategoryId = 5, Name = "Category5", ParentCategoryId = 4 });

            mockCategoryService.Setup(m => m.GetAllAsync()).ReturnsAsync(categories);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mockSearchFilterService.Object, mapper);

            int categoryId = 5;
            var result = await controller.RetrivePreviousSelectedCategories(categoryId, It.IsAny<string>(), It.IsAny<string>()) as PartialViewResult;
            var resultModel = (IEnumerable<CategoryDropDownViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(5, resultModel.Count());
            Assert.AreEqual("Category1", resultModel.ToArray()[0].Name);
            Assert.AreEqual("Category2", resultModel.ToArray()[1].Name);
            Assert.AreEqual("Category3", resultModel.ToArray()[2].Name);
            Assert.AreEqual("Category4", resultModel.ToArray()[3].Name);
            Assert.AreEqual("Category5", resultModel.ToArray()[4].Name);
            Assert.AreEqual("_PreviousSelectedCategoriesPartial", result.ViewName);
        }

        [Test]
        public async Task RetrivePreviousSelectedCategories_pass_values_to_viewbags()
        {
            mockCategoryService.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<Category>());

            CategoryController controller = new CategoryController(mockCategoryService.Object, mockSearchFilterService.Object, mapper);

            string actionName = "testActionName";
            string areaName = "testAreaName";

            var result = await controller.RetrivePreviousSelectedCategories(null, actionName, areaName) as PartialViewResult;
            var resultModel = (IEnumerable<CategoryDropDownViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(actionName, result.ViewBag.TargetActionName);
            Assert.AreEqual(areaName, result.ViewBag.AreaName);
        }
        #endregion
    }
}
