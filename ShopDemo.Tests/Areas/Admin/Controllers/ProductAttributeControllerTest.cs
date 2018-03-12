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
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class ProductAttributeControllerTest
    {
        Mock<IProductAttributeService> mockProductAttributeService;
        IMapper mapper;

        List<ProductAttribute> productAttributesList;

        [SetUp]
        public void SetUp()
        {
            mockProductAttributeService = new Mock<IProductAttributeService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductAttributeProfile());

            });
            mapper = config.CreateMapper();

            productAttributesList = new List<ProductAttribute>();
            productAttributesList.Add(new ProductAttribute { ProductAttributeId = 1, Name = "Attrib1", CategoryId = 1 });
            productAttributesList.Add(new ProductAttribute { ProductAttributeId = 2, Name = "Attrib2", CategoryId = 1 });
            productAttributesList.Add(new ProductAttribute { ProductAttributeId = 3, Name = "Attrib3", CategoryId = 2 });
            productAttributesList.Add(new ProductAttribute { ProductAttributeId = 4, Name = "Attrib4", CategoryId = 3 });
            productAttributesList.Add(new ProductAttribute { ProductAttributeId = 5, Name = "Attrib1", CategoryId = 2 });
        }

        [Test]
        public async Task GetProductAttributes_returns_partial_view_with_product_attributes()
        {
            mockProductAttributeService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductAttribute, bool>>>())).ReturnsAsync(productAttributesList.Where(c => c.CategoryId == 1));

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.GetProductAttributes(It.IsAny<int>()) as PartialViewResult;
            var resultModel = (IEnumerable<ProductAttributeViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, resultModel.Count());
            Assert.AreEqual("Attrib1", resultModel.ToArray()[0].Name);
            Assert.AreEqual("Attrib2", resultModel.ToArray()[1].Name);
            Assert.AreEqual("_ProductAttributesPartial", result.ViewName);
        }

        #region GetParentCategoryProductAttributesDistinct
        [Test]
        public async Task GetParentCategoryProductAttributesDistinct_returns_partial_view_with_null_model_if_parentCategoryId_is_equal_to_null()
        {
            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.GetParentCategoryProductAttributesDistinct(It.IsAny<int>(),null) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.IsNull(result.Model);
            Assert.AreEqual("_ParentCategoryProductAttributesPartial", result.ViewName);
        }

        [Test]
        public async Task GetParentCategoryProductAttributesDistinct_returns_partial_view_with_distinct_product_attributes_if_parentCategoryId_is_not_null()
        {
            IQueryable<ProductAttribute> queryableAttributes = productAttributesList.AsQueryable();

            mockProductAttributeService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductAttribute, bool>>>())).ReturnsAsync((Expression<Func<ProductAttribute, bool>> predicate) =>
            {
                return queryableAttributes.Where(predicate);
            });

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.GetParentCategoryProductAttributesDistinct(1, 2) as PartialViewResult;
            var resultModel = (IEnumerable<ParentCategoryProductAtributeViewModel>)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, resultModel.Count());
            Assert.AreEqual("Attrib3", resultModel.ToArray()[0].Name);
            Assert.AreEqual("_ParentCategoryProductAttributesPartial", result.ViewName);
        }
        #endregion

        #region AddProductAttributes
        [Test]
        public async Task AddProductAttributes_return_error_message_as_json_result_if_model_state_is_not_valid()
        {
            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.AddProductAttributes(It.IsAny<AddProductAttributesViewModel>()) as JsonResult;

            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task AddProductAttributes_adds_product_attributes()
        {
            mockProductAttributeService.Setup(m => m.AddMultipleProductAttributesAsync(It.IsAny<IEnumerable<ProductAttribute>>())).Returns(Task.FromResult(true));

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            string[] attributes = { };
            var result = await controller.AddProductAttributes(new AddProductAttributesViewModel { CategoryId = 1, Attributes = attributes }) as JsonResult;

            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockProductAttributeService.Verify(m => m.AddMultipleProductAttributesAsync(It.IsAny<IEnumerable<ProductAttribute>>()), Times.Once);
        }
        #endregion

        #region RemoveProductAttribute
        [Test]
        public async Task RemoveProductAttribute_returns_error_message_as_json_result_if_productAttribute_is_null()
        {
            ProductAttribute productAttribute = null;
            mockProductAttributeService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productAttribute);

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.RemoveProductAttribute(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task RemoveProductAttribute_remove_product_attribute_if_productAttribute_is_not_null()
        {
            mockProductAttributeService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new ProductAttribute());
            mockProductAttributeService.Setup(m => m.DeleteAsync(It.IsAny<ProductAttribute>())).Returns(Task.FromResult(true));

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.RemoveProductAttribute(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockProductAttributeService.Verify(m => m.DeleteAsync(It.IsAny<ProductAttribute>()), Times.Once);
        }
        #endregion

        #region EditProductAttribute
        [Test]
        public async Task EditProductAttribute_returns_error_message_as_json_result_if_productAttribute_not_exists()
        {
            mockProductAttributeService.Setup(m => m.ProductAttributeExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.EditProductAttribute(new EditProductAttributeViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task EditProductAttribute_updates_product_attribute_if_productAttribute_exists()
        {
            mockProductAttributeService.Setup(m => m.ProductAttributeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            mockProductAttributeService.Setup(m => m.UpdateAsync(It.IsAny<ProductAttribute>())).Returns(Task.FromResult(true));

            ProductAttributeController controller = new ProductAttributeController(mockProductAttributeService.Object, mapper);

            var result = await controller.EditProductAttribute(new EditProductAttributeViewModel()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockProductAttributeService.Verify(m => m.UpdateAsync(It.IsAny<ProductAttribute>()), Times.Once);
        }
        #endregion
    }
}