using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.Controllers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class ProductRateControllerTest
    {
        Mock<IProductRateService> mockRateService;
        IMapper mapper;
        List<ProductRate> rates;

        [SetUp]
        public void SetUp()
        {
            mockRateService = new Mock<IProductRateService>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductRateProfile());

            });
            mapper = config.CreateMapper();

            rates = new List<ProductRate>();
            rates.Add(new ProductRate { ProductRateId = 1, ProductId = 1, Comment = "comment1", CreatedAt = DateTime.Now, NickName = "NickName1", Rate = 5 });
            rates.Add(new ProductRate { ProductRateId = 2, ProductId = 1, Comment = "comment2", CreatedAt = DateTime.Now, NickName = "NickName2", Rate = 2 });
            rates.Add(new ProductRate { ProductRateId = 3, ProductId = 1, Comment = "comment3", CreatedAt = DateTime.Now, NickName = "NickName3", Rate = 1 });
            rates.Add(new ProductRate { ProductRateId = 4, ProductId = 1, Comment = "comment4", CreatedAt = DateTime.Now, NickName = "NickName4", Rate = 4 });
        }

        [Test]
        public async Task GetAllProductComments_returns_ProductCommentsNotExistsPartial_if_comments_count_is_not_greater_than_zero()
        {
            List<ProductRate> productRates = new List<ProductRate>();
            mockRateService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductRate, bool>>>())).Returns(Task.FromResult(productRates.AsEnumerable()));

            ProductRateController controller = new ProductRateController(mockRateService.Object, mapper);

            var result = await controller.GetAllProductComments(It.IsAny<int>()) as PartialViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("_ProductCommentsNotExistsPartial", result.ViewName);
        }

        [Test]
        public async Task GetAllProductComments_returns_ProductCommentsPartial_with_correct_data_if_comments_count_is_greater_than_zero()
        {
            mockRateService.Setup(m => m.GetAllAsync(It.IsAny<Expression<Func<ProductRate, bool>>>())).Returns(Task.FromResult(rates.AsEnumerable()));

            ProductRateController controller = new ProductRateController(mockRateService.Object, mapper);

            var result = await controller.GetAllProductComments(It.IsAny<int>()) as PartialViewResult;
            var ratesModel = (ProductRateListViewModel)result.Model;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("_ProductCommentsPartial", result.ViewName);

            Assert.AreEqual(4, ratesModel.Comments.Count);
            Assert.AreEqual(3, ratesModel.TotalRate);
            Assert.AreEqual("comment1", ratesModel.Comments[0].Comment);
            Assert.AreEqual("NickName1", ratesModel.Comments[0].NickName);
        }
    }
}
