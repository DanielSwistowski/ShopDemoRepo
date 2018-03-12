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
    public class SearchFilterControllerTest
    {
        Mock<ISearchFilterService> mockSearchFilterService;
        IMapper mapper;
        List<SearchFilter> searchFiltersList;

        [SetUp]
        public void SetUp()
        {
            mockSearchFilterService = new Mock<ISearchFilterService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new SearchFilterProfile());

            });
            mapper = config.CreateMapper();

            searchFiltersList = new List<SearchFilter>();

            List<ProductAttributeValue> attrib1Values = new List<ProductAttributeValue> {
                new ProductAttributeValue { ProductAttributeId = 1, AttributeValue = "Attrib1Value1" },
                new ProductAttributeValue { ProductAttributeId = 1, AttributeValue = "Attrib1Value2" } };

            searchFiltersList.Add(new SearchFilter
            {
                CategoryId = 1,
                ProductAttributeId = 1,
                FilterType = FilterType.CheckboxList,
                ProductAttribute = new ProductAttribute { ProductAttributeId = 1, Name = "Attrib1", ProductAttributeValues = attrib1Values }
            });

            List<ProductAttributeValue> attrib2Values = new List<ProductAttributeValue> {
                new ProductAttributeValue { ProductAttributeId = 2, AttributeValue = "Attrib2Value1" },
                new ProductAttributeValue { ProductAttributeId = 2, AttributeValue = "Attrib2Value2" } };

            searchFiltersList.Add(new SearchFilter
            {
                CategoryId = 1,
                ProductAttributeId = 2,
                FilterType = FilterType.CheckboxList,
                ProductAttribute = new ProductAttribute { ProductAttributeId = 2, Name = "Attrib2", ProductAttributeValues = attrib2Values }
            });
        }

        [Test]
        public async Task GetProductSearchFiltersPartial_returns_partial_view_ProductSearchFiltersPartial_with_search_filters_collection()
        {
            mockSearchFilterService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<SearchFilter, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(searchFiltersList);

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mapper);

            var result = await controller.GetProductSearchFiltersPartial(It.IsAny<int>(), It.IsAny<string>()) as PartialViewResult;
            var resultModel = (List<SearchFiltersForProductViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual("_ProductSearchFiltersPartial", result.ViewName);
            Assert.AreEqual(2, resultModel[0].AttributeValues.Count());
            Assert.AreEqual(2, resultModel[1].AttributeValues.Count());
            Assert.AreEqual("Attrib1", resultModel[0].Attribute);
            Assert.AreEqual("Attrib2", resultModel[1].Attribute);
        }

        [Test]
        public async Task GetProductSearchFiltersPartial_set_attribute_value_as_selected_if_value_exists_in_currentSelectedParms_string()
        {
            mockSearchFilterService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<SearchFilter, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(searchFiltersList);

            SearchFilterController controller = new SearchFilterController(mockSearchFilterService.Object, mapper);

            string currentSelectedParams = "Attrib1-Attrib1Value1";
            var result = await controller.GetProductSearchFiltersPartial(It.IsAny<int>(), currentSelectedParams) as PartialViewResult;
            var resultModel = (List<SearchFiltersForProductViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.IsTrue(resultModel[0].AttributeValues.First().IsSelected);
        }
    }
}
