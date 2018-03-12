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
    public class SearchFilterControllerTest
    {
        Mock<ISearchFilterService> mockSearchFilterService;
        Mock<IProductAttributeService> mockProductAttributeService;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockSearchFilterService = new Mock<ISearchFilterService>();
            mockProductAttributeService = new Mock<IProductAttributeService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new SearchFilterProfile());

            });
            mapper = config.CreateMapper();
        }

        [Test]
        public async Task GetSearchFilters_returns_partial_view_ProductSearchFiltersManagementPartial_with_model()
        {
            List<SearchFilter> searchFiltersList = new List<SearchFilter>();
            searchFiltersList.Add(new SearchFilter { CategoryId = 1, ProductAttributeId = 1, FilterType = FilterType.CheckboxList, ProductAttribute = new ProductAttribute { ProductAttributeId = 1, Name = "Attrib1" } });
            searchFiltersList.Add(new SearchFilter { CategoryId = 1, ProductAttributeId = 2, FilterType = FilterType.CheckboxList, ProductAttribute = new ProductAttribute { ProductAttributeId = 2, Name = "Attrib2" } });
            mockSearchFilterService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<SearchFilter, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(searchFiltersList);

            List<ProductAttribute> productAttributesList = new List<ProductAttribute>();
            productAttributesList.Add(new ProductAttribute { CategoryId = 1, ProductAttributeId = 3, Name = "Attrib3" });
            productAttributesList.Add(new ProductAttribute { CategoryId = 1, ProductAttributeId = 4, Name = "Attrib4" });
            mockProductAttributeService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductAttribute, bool>>>())).ReturnsAsync(productAttributesList);

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);

            var result = await controller.GetSearchFilters(It.IsAny<int>()) as PartialViewResult;
            var resultModel = (SearchFilterListAndAddFilterViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual("_ProductSearchFiltersManagementPartial", result.ViewName);
            Assert.AreEqual(2, resultModel.SearchFiltersList.Count());
            Assert.AreEqual(2, resultModel.AddSearchFilterModel.AttributesList.Count());
            Assert.AreEqual("Attrib1", resultModel.SearchFiltersList.Select(s => s.Attribute).First());
            Assert.AreEqual("Attrib4", resultModel.AddSearchFilterModel.AttributesList.Select(s => s.Text).Last());
        }

        #region AddSearchFilter
        [Test]
        public async Task AddSearchFilter_returns_error_message_as_json_if_model_state_is_not_valid()
        {
            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.AddSearchFilter(It.IsAny<AddSearchFilterViewModel>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task AddSearchFilter_returns_error_message_as_json_search_filter_exists()
        {
            mockSearchFilterService.Setup(m => m.SearchFilterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);

            var result = await controller.AddSearchFilter(new AddSearchFilterViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task AddSearchFilter_adds_new_search_filter()
        {
            mockSearchFilterService.Setup(m => m.SearchFilterExistsAsync(It.IsAny<int>())).ReturnsAsync(false);
            mockSearchFilterService.Setup(m => m.AddAsync(It.IsAny<SearchFilter>())).Returns(Task.FromResult(true));

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);

            var result = await controller.AddSearchFilter(new AddSearchFilterViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockSearchFilterService.Verify(m => m.AddAsync(It.IsAny<SearchFilter>()), Times.Once);
        }
        #endregion

        #region DeleteSearchFilter
        [Test]
        public async Task DeleteSearchFilter_returns_error_message_as_json_if_search_filter_is_null()
        {
            SearchFilter searchFilter = null;
            mockSearchFilterService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(searchFilter);

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);

            var result = await controller.DeleteSearchFilter(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteSearchFilter_removes_search_filter()
        {
            mockSearchFilterService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new SearchFilter());
            mockSearchFilterService.Setup(m => m.DeleteAsync(It.IsAny<SearchFilter>())).Returns(Task.FromResult(true));

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mockProductAttributeService.Object, mapper);

            var result = await controller.DeleteSearchFilter(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockSearchFilterService.Verify(m => m.DeleteAsync(It.IsAny<SearchFilter>()),Times.Once);
        }
        #endregion
    }
}