using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class CategoryControllerTest
    {
        Mock<ICategoryService> mockCategoryService;
        IMapper mapper;

        List<Category> categories;

        [SetUp]
        public void SetUp()
        {
            mockCategoryService = new Mock<ICategoryService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryProfile());

            });
            mapper = config.CreateMapper();

            categories = new List<Category>();
            categories.Add(new Category { CategoryId = 1, Name = "Komputery", ParentCategoryId = null });
            categories.Add(new Category { CategoryId = 2, Name = "Stacjonarne", ParentCategoryId = 1 });
            categories.Add(new Category { CategoryId = 3, Name = "Podzespoły bazowe", ParentCategoryId = 2 });
            categories.Add(new Category { CategoryId = 4, Name = "Płyty główne", ParentCategoryId = 3 });
            categories.Add(new Category { CategoryId = 5, Name = "Procesory", ParentCategoryId = 3 });
            categories.Add(new Category { CategoryId = 6, Name = "Dyski twarde", ParentCategoryId = 3 });
            categories.Add(new Category { CategoryId = 7, Name = "Pamięć ram", ParentCategoryId = 3 });
            categories.Add(new Category { CategoryId = 8, Name = "Zasilacze", ParentCategoryId = 3 });
        }

        [Test]
        public async Task GetCategoriesJson_returns_correct_data()
        {
            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync(categories.Where(c => c.ParentCategoryId == 3));

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.GetCategoriesJson(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;
            var categoriesModel = (IEnumerable<CategoryViewModel>)jsonData;

            Assert.IsNotNull(result);
            Assert.AreEqual(5, categoriesModel.Count());
            Assert.AreEqual("Płyty główne", categoriesModel.Select(c => c.Name).First());
        }

        #region AddCategory
        [Test]
        public async Task AddCategory_returns_error_message_if_model_state_is_not_valid()
        {
            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.AddCategory(new AddCategoryViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task AddCategory_adds_new_category()
        {
            mockCategoryService.Setup(m => m.AddAsync(new Category()));

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.AddCategory(new AddCategoryViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockCategoryService.Verify(m => m.AddAsync(It.IsAny<Category>()), Times.Once);
        }
        #endregion

        #region EditCategory
        [Test]
        public async Task EditCategory_returns_error_message_if_model_state_is_not_valid()
        {
            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.EditCategory(new EditCategoryViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task EditCategory_returns_error_message_if_category_is_null()
        {
            Category category = null;
            mockCategoryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(category);
            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.EditCategory(new EditCategoryViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task EditCategory_updates_category_data()
        {
            mockCategoryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Category());
            mockCategoryService.Setup(m => m.UpdateAsync(new Category()));

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.EditCategory(new EditCategoryViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockCategoryService.Verify(m => m.UpdateAsync(It.IsAny<Category>()), Times.Once);
        }
        #endregion


        #region DeleteCategory
        [Test]
        public async Task DeleteCategory_returns_error_message_if_category_exists_as_parent_category()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int>())).ReturnsAsync(true);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.DeleteCategory(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteCategory_returns_error_message_if_category_contains_assigned_products()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.CategoryContainsProductsAsync(It.IsAny<int>())).ReturnsAsync(true);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.DeleteCategory(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteCategory_returns_error_message_if_category_is_null()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.CategoryContainsProductsAsync(It.IsAny<int>())).ReturnsAsync(false);
            Category category = null;
            mockCategoryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(category);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.DeleteCategory(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteCategory_removed_category()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.CategoryContainsProductsAsync(It.IsAny<int>())).ReturnsAsync(false);
            mockCategoryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Category());
            mockCategoryService.Setup(m => m.DeleteAsync(new Category()));

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.DeleteCategory(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockCategoryService.Verify(m => m.DeleteAsync(It.IsAny<Category>()), Times.Once);
        }
        #endregion

        [Test]
        public async Task GetCategoriesForDropDownJson_returns_correct_data()
        {
            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync(categories.Where(c => c.ParentCategoryId == 3));

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.GetCategoriesForDropDownJson(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;
            var categoriesModel = (IEnumerable<CategoryDropDownViewModel>)jsonData;

            Assert.IsNotNull(result);
            Assert.AreEqual(5, categoriesModel.Count());
            Assert.AreEqual("Płyty główne", categoriesModel.Select(c => c.Name).First());
        }

        [Test]
        public async Task CategoryExistsAsParentJson_returns_correct_json_result()
        {
            mockCategoryService.Setup(m => m.CategoryExistsAsParentAsync(It.IsAny<int>())).ReturnsAsync(true);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.CategoryExistsAsParentJson(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData);
        }

        #region ResolveSelectedCategoriesForDropDown
        [Test]
        public async Task ResolveSelectedCategoriesForDropDown_returns_null_if_selectedCategories_parm_is_null()
        {
            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.ResolveSelectedCategoriesForDropDown(null) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsNull(jsonData);
        }

        [Test]
        public async Task ResolveSelectedCategoriesForDropDown_returns_correct_data()
        {
            IQueryable<Category> queryableCategories = categories.AsQueryable();

            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync((Expression<Func<Category, bool>> filter) =>
                {
                    return queryableCategories.Where(filter);
                });

            List<int> selectedCategories = new List<int>();
            selectedCategories.Add(1);
            selectedCategories.Add(2);
            selectedCategories.Add(3);
            selectedCategories.Add(8);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.ResolveSelectedCategoriesForDropDown(selectedCategories) as JsonResult;
            dynamic jsonData = result.Data;
            var categoriesModel = (List<SelectedCategoryViewModel>)jsonData;

            Assert.IsNotNull(result);
            Assert.AreEqual(4, categoriesModel.Count());
            Assert.IsNotNull(categoriesModel[0].Categories);
            Assert.IsTrue(categoriesModel[0].SelectedCategoryId == 1);

            Assert.IsNotNull(categoriesModel[1].Categories);
            Assert.IsTrue(categoriesModel[1].SelectedCategoryId == 2);

            Assert.IsNotNull(categoriesModel[2].Categories);
            Assert.IsTrue(categoriesModel[2].SelectedCategoryId == 3);

            Assert.IsNotNull(categoriesModel[3].Categories);
            Assert.IsTrue(categoriesModel[3].SelectedCategoryId == 8);
        }
        #endregion

        #region FindProductCategoriesByProductName
        [Test]
        public async Task FindProductCategoriesByProductName_returns_json_data_equals_false_if_categoriesIds_is_null()
        {
            IEnumerable<int> categoriesIds = null;
            mockCategoryService.Setup(m => m.FindProductCategoriesIdsByProductNameAsync(It.IsAny<string>())).ReturnsAsync(categoriesIds);

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.FindProductCategoriesByProductName(It.IsAny<string>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.data);
        }

        [Test]
        public async Task FindProductCategoriesByProductName_returns_correct_data()
        {
            List<int> categoriesIds = new List<int>();
            categoriesIds.Add(1);
            categoriesIds.Add(2);
            categoriesIds.Add(3);
            categoriesIds.Add(8);

            mockCategoryService.Setup(m => m.FindProductCategoriesIdsByProductNameAsync(It.IsAny<string>())).ReturnsAsync(categoriesIds);

            IQueryable<Category> queryableCategories = categories.AsQueryable();

            mockCategoryService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>())).ReturnsAsync((Expression<Func<Category, bool>> filter) =>
            {
                return queryableCategories.Where(filter);
            });

            CategoryController controller = new CategoryController(mockCategoryService.Object, mapper);

            var result = await controller.FindProductCategoriesByProductName(It.IsAny<string>()) as JsonResult;
            dynamic jsonData = result.Data;

            var categoriesModel = (List<SelectedCategoryViewModel>)jsonData.data;

            Assert.IsNotNull(result);
            Assert.AreEqual(4, categoriesModel.Count());
            Assert.IsNotNull(categoriesModel[0].Categories);
            Assert.IsTrue(categoriesModel[0].SelectedCategoryId == 1);

            Assert.IsNotNull(categoriesModel[1].Categories);
            Assert.IsTrue(categoriesModel[1].SelectedCategoryId == 2);

            Assert.IsNotNull(categoriesModel[2].Categories);
            Assert.IsTrue(categoriesModel[2].SelectedCategoryId == 3);

            Assert.IsNotNull(categoriesModel[3].Categories);
            Assert.IsTrue(categoriesModel[3].SelectedCategoryId == 8);
        }
        #endregion
    }
}