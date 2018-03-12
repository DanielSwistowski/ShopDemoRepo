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
    public class ProductAttributeValueControllerTest
    {
        Mock<IProductAttributeValueService> mockProductAttributeValueService;
        IMapper mapper;
        List<ProductAttributeValue> attributeValuesList;
        List<ProductAttributeValue> parentAttributeValues;

        [SetUp]
        public void SetUp()
        {
            mockProductAttributeValueService = new Mock<IProductAttributeValueService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductAttributeValueProfile());

            });
            mapper = config.CreateMapper();

            attributeValuesList = new List<ProductAttributeValue>();
            attributeValuesList.Add(new ProductAttributeValue { ProductAttributeValueId = 1, ProductAttributeId = 1, AttributeValue = "value1" });
            attributeValuesList.Add(new ProductAttributeValue { ProductAttributeValueId = 2, ProductAttributeId = 2, AttributeValue = "value2" });

            parentAttributeValues = new List<ProductAttributeValue>();
            parentAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 1, ProductAttributeId = 1, AttributeValue = "value1" });
            parentAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 2, ProductAttributeId = 2, AttributeValue = "value2" });
            parentAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 2, ProductAttributeId = 2, AttributeValue = "value3" });

        }

        [Test]
        public async Task GetParentProductAttributeValuesDistinct_returns_product_attribute_values_distinct()
        {
            mockProductAttributeValueService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductAttributeValue, bool>>>())).ReturnsAsync(attributeValuesList);
            mockProductAttributeValueService.Setup(m => m.GetProductAttributeValuesByCategoryIdAndAttributeNameAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(parentAttributeValues);

            ProductAttributeValueController controller = new ProductAttributeValueController(mockProductAttributeValueService.Object, mapper);

            var result = await controller.GetParentProductAttributeValuesDistinct(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()) as PartialViewResult;
            var resultModel = (IEnumerable<ParentAttributeValueViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultModel.Count());
            Assert.AreEqual("value3", resultModel.ToArray()[0].AttributeValue);
            Assert.AreEqual("_ParentProductAttributeValuesPartial", result.ViewName);
        }

        #region AddProductAttributeValue
        [Test]
        public async Task AddProductAttributeValue_return_error_message_as_json_result_if_model_state_is_not_valid()
        {
            ProductAttributeValueController controller = new ProductAttributeValueController(mockProductAttributeValueService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.AddProductAttributeValue(It.IsAny<AddProductAttributeValueViewModel>()) as JsonResult;

            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task AddProductAttributeValue_adds_product_attribute_values()
        {
            mockProductAttributeValueService.Setup(m => m.AddMultipleProductAttributeValuesAsync(It.IsAny<IEnumerable<ProductAttributeValue>>())).Returns(Task.FromResult(true));

            ProductAttributeValueController controller = new ProductAttributeValueController(mockProductAttributeValueService.Object, mapper);

            string[] attributeValues = { };
            var result = await controller.AddProductAttributeValue(new AddProductAttributeValueViewModel { AttributeValues = attributeValues }) as JsonResult;

            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockProductAttributeValueService.Verify(m => m.AddMultipleProductAttributeValuesAsync(It.IsAny<IEnumerable<ProductAttributeValue>>()), Times.Once);
        }
        #endregion

        #region RemoveProductAttributeValue
        [Test]
        public async Task RemoveProductAttributeValue_returns_error_message_as_json_result_if_attributeValue_is_null()
        {
            ProductAttributeValue productAttributeValue = null;
            mockProductAttributeValueService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productAttributeValue);

            ProductAttributeValueController controller = new ProductAttributeValueController(mockProductAttributeValueService.Object, mapper);

            var result = await controller.RemoveProductAttributeValue(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task RemoveProductAttributeValue_remove_product_attribute_value_if_attributeValue_is_not_null()
        {
            mockProductAttributeValueService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProductAttributeValue());
            mockProductAttributeValueService.Setup(m => m.DeleteAsync(It.IsAny<ProductAttributeValue>())).Returns(Task.FromResult(true));

            ProductAttributeValueController controller = new ProductAttributeValueController(mockProductAttributeValueService.Object, mapper);

            var result = await controller.RemoveProductAttributeValue(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockProductAttributeValueService.Verify(m => m.DeleteAsync(It.IsAny<ProductAttributeValue>()), Times.Once);
        }
        #endregion
    }
}
