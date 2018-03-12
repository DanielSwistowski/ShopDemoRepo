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
    public class ProductRateControllerTest
    {
        Mock<IProductRateService> mockProductRateService;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockProductRateService = new Mock<IProductRateService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductRateProfile());

            });
            mapper = config.CreateMapper();
        }

        #region GetAllProductComments
        [Test]
        public async Task GetAllProductComments_returns_partial_view_ProductCommentsNotExistsPartial_if_comments_count_is_not_greather_then_0()
        {
            List<ProductRate> rates = new List<ProductRate>();
            mockProductRateService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductRate, bool>>>())).ReturnsAsync(rates);

            ProductRateController controller = new ProductRateController(mockProductRateService.Object, mapper);

            var result = await controller.GetAllProductComments(It.IsAny<int>()) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_ProductCommentsNotExistsPartial", result.ViewName);
        }

        [Test]
        public async Task GetAllProductComments_returns_partial_view_ProductCommentsPartial_with_view_model_if_comments_count_is_greather_then_0()
        {
            List<ProductRate> rates = new List<ProductRate>();
            rates.Add(new ProductRate());
            mockProductRateService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductRate, bool>>>())).ReturnsAsync(rates);

            ProductRateController controller = new ProductRateController(mockProductRateService.Object, mapper);

            var result = await controller.GetAllProductComments(It.IsAny<int>()) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Model);
            Assert.IsInstanceOf<ProductRateListViewModel>(result.Model);
            Assert.AreEqual("_ProductCommentsPartial", result.ViewName);
        }
        #endregion

        #region DeleteComment
        [Test]
        public async Task DeleteComment_returns_error_message_as_json_result_if_comment_not_exists()
        {
            ProductRate productRate = null;
            mockProductRateService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productRate);

            ProductRateController controller = new ProductRateController(mockProductRateService.Object, mapper);

            var result = await controller.DeleteComment(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteComment_removes_productRate_and_returns_message_as_json_result()
        {
            ProductRate productRate = new ProductRate();
            mockProductRateService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productRate);
            mockProductRateService.Setup(m => m.DeleteAsync(It.IsAny<ProductRate>())).Returns(Task.FromResult(true));

            ProductRateController controller = new ProductRateController(mockProductRateService.Object, mapper);

            var result = await controller.DeleteComment(It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            Assert.IsNotNull(jsonData.message);
            mockProductRateService.Verify(m => m.DeleteAsync(It.IsAny<ProductRate>()),Times.Once);
        }
        #endregion
    }
}