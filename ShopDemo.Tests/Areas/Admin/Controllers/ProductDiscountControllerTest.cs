using AutoMapper;
using DataAccessLayer.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
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
    public class ProductDiscountControllerTest
    {
        Mock<IProductService> mockProductService;
        Mock<IProductDiscountService> mockProductDiscountService;
        Mock<IBackgroundJobClient> mockJobClient;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockProductService = new Mock<IProductService>();
            mockProductDiscountService = new Mock<IProductDiscountService>();
            mockJobClient = new Mock<IBackgroundJobClient>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductDiscountProfile());
            });
            mapper = config.CreateMapper();
        }

        #region Details
        [Test]
        public async Task Details_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Details_returns_NotFound_if_product_is_null()
        {
            ProductDiscount nullProductDiscount = null;
            mockProductDiscountService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<ProductDiscount, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullProductDiscount);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_returns_correct_product_details_data()
        {
            ProductDiscount productDiscount = new ProductDiscount { ProductId = 1, Status = ProductDiscountStatus.DuringTime, Product = new Product { ProductId = 1, Name = "ProductName", Price = 456 } };

            mockProductDiscountService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<ProductDiscount, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(productDiscount);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Details(10) as ViewResult;
            var resultModel = (ProductDiscountDetailsViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual((int)ProductDiscountStatusViewModel.DuringTime, (int)resultModel.Status);
            Assert.AreEqual("ProductName", resultModel.BasicProductDataViewModel.Name);
            Assert.AreEqual(456, resultModel.BasicProductDataViewModel.Price);
        }
        #endregion

        #region Add
        [Test]
        public async Task Add_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Add() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Add_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullProduct);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Add(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Add_returns_view_with_model()
        {
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Add(10) as ViewResult;

            Assert.NotNull(result);
            Assert.NotNull((AddProductDiscountViewModel)result.Model);
        }

        [Test]
        public async Task Add_returns_model_state_error_if_model_state_is_invalid()
        {
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.Add(new AddProductDiscountViewModel { BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 } }) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Add_returns_model_state_error_if_productDiscount_is_not_null_and_discount_status_is_equal_to_DuringTime()
        {
            ProductDiscount productDiscount = new ProductDiscount { Status = ProductDiscountStatus.DuringTime };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Add(new AddProductDiscountViewModel { BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 } }) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Add_returns_model_state_error_if_productDiscount_is_not_null_and_discount_status_is_equal_to_WaitingForStart()
        {
            ProductDiscount productDiscount = new ProductDiscount { Status = ProductDiscountStatus.WaitingForStart };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.Add(new AddProductDiscountViewModel { BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 } }) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Add_removes_old_product_discount_and_create_new_if_discount_status_is_not_equal_to_DuringTime_or_WaitingForStart()
        {
            ProductDiscount productDiscount = new ProductDiscount { Status = ProductDiscountStatus.Ended };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductDiscountService.Setup(m => m.DeleteAsync(It.IsAny<ProductDiscount>())).Returns(Task.FromResult(true));

            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>())).Returns("1");
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>())).Returns("2");

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var time = new DateTime(2017, 12, 20, 12, 12, 12);
            AddProductDiscountViewModel addProductDiscountViewModel = new AddProductDiscountViewModel
            {
                DiscountStartTime = time,
                DiscountEndTime = time.AddDays(1),
                BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 }
            };

            await controller.Add(addProductDiscountViewModel);

            mockProductDiscountService.Verify(m => m.DeleteAsync(It.IsAny<ProductDiscount>()), Times.Once);
            mockProductDiscountService.Verify(m => m.AddAsync(It.IsAny<ProductDiscount>()), Times.Once);
            mockJobClient.Verify(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>()), Times.Once);
            mockJobClient.Verify(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>()), Times.Once);
        }

        [Test]
        public async Task Add_removes_hangfire_jobs_and_returns_model_state_error_if_AddAsync_method_throws_exception()
        {
            ProductDiscount productDiscount = new ProductDiscount { Status = ProductDiscountStatus.Ended };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductDiscountService.Setup(m => m.DeleteAsync(It.IsAny<ProductDiscount>())).Returns(Task.FromResult(true));
            mockProductDiscountService.Setup(m => m.AddAsync(It.IsAny<ProductDiscount>())).ThrowsAsync(new Exception());
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>())).Returns("1");
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>())).Returns("2");

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var time = new DateTime(2017, 12, 20, 12, 12, 12);
            AddProductDiscountViewModel addProductDiscountViewModel = new AddProductDiscountViewModel
            {
                DiscountStartTime = time,
                DiscountEndTime = time.AddDays(1),
                BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 }
            };

            var result = await controller.Add(addProductDiscountViewModel) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);

            mockJobClient.Verify(m => m.ChangeState("1", It.IsAny<DeletedState>(), null), Times.Once);
            mockJobClient.Verify(m => m.ChangeState("2", It.IsAny<DeletedState>(), null), Times.Once);
        }

        [Test]
        public async Task Add_adds_new_product_discount_and_redirects_to_ProductsOnPromotion_action_in_Product_controller()
        {
            ProductDiscount productDiscount = null;
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductDiscountService.Setup(m => m.AddAsync(It.IsAny<ProductDiscount>())).Returns(Task.FromResult(true));

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var time = new DateTime(2017, 12, 20, 12, 12, 12);
            AddProductDiscountViewModel addProductDiscountViewModel = new AddProductDiscountViewModel
            {
                DiscountStartTime = time,
                DiscountEndTime = time.AddDays(1),
                BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 }
            };

            var result = await controller.Add(addProductDiscountViewModel) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ProductsOnPromotion", result.RouteValues["action"]);
            Assert.AreEqual("Product", result.RouteValues["controller"]);
            mockProductDiscountService.Verify(m => m.AddAsync(It.IsAny<ProductDiscount>()), Times.Once);
        }
        #endregion

        #region GetNewProductPrice
        [Test]
        public async Task GetNewProductPrice_returns_error_message_as_json_result_if_productId_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetNewProductPrice(It.IsAny<int>(), 10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task GetNewProductPrice_returns_error_message_as_json_result_if_discountQuantity_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetNewProductPrice(10, It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task GetNewProductPrice_returns_product_promotion_price()
        {
            mockProductDiscountService.Setup(m => m.CalculateNewProductPriceAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(200);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.GetNewProductPrice(10, 10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(200, jsonData.price);
        }
        #endregion

        #region EditProductDiscount
        [Test]
        public async Task EditProductDiscount_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.EditProductDiscount() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task EditProductDiscount_returns_NotFound_if_productDiscount_is_null()
        {
            ProductDiscount nullProductDiscount = null;
            mockProductDiscountService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<ProductDiscount, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullProductDiscount);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.EditProductDiscount(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task EditProductDiscount_returns_view_with_model()
        {
            mockProductDiscountService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<ProductDiscount, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new ProductDiscount());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.EditProductDiscount(10) as ViewResult;

            Assert.NotNull(result);
            Assert.NotNull((EditProductDiscountViewModel)result.Model);
        }

        [Test]
        public async Task EditProductDiscount_returns_model_state_error_if_model_state_is_invalid()
        {
            mockProductDiscountService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<ProductDiscount, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new ProductDiscount());

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.EditProductDiscount(new EditProductDiscountViewModel { BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 } }) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EditProductDiscount_removes_new_hangfire_jobs_and_returns_model_state_error_if_UpdateAsync_method_throws_exception()
        {
            ProductDiscount productDiscount = new ProductDiscount { ProductId = 1 };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockProductDiscountService.Setup(m => m.UpdateAsync(It.IsAny<ProductDiscount>())).ThrowsAsync(new Exception());
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>())).Returns("1");
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>())).Returns("2");

            var time = new DateTime(2017, 12, 20, 12, 12, 12);
            EditProductDiscountViewModel editProductDiscountViewModel = new EditProductDiscountViewModel
            {
                DiscountStartTime = time,
                DiscountEndTime = time.AddDays(1),
                BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 }
            };

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.EditProductDiscount(editProductDiscountViewModel) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
            mockJobClient.Verify(m => m.ChangeState("1", It.IsAny<DeletedState>(), null), Times.Once);
            mockJobClient.Verify(m => m.ChangeState("2", It.IsAny<DeletedState>(), null), Times.Once);
        }

        [Test]
        public async Task EditProductDiscount_creates_new_and_removes_old_hangfire_jobs_and_redirects_to_ProductsOnPromotion_action_in_Product_controller_if_UpdateAsync_method_is_succeeded()
        {
            ProductDiscount productDiscount = new ProductDiscount { ProductId = 1, StartDiscountJobId = "oldStartJobId", StopDiscountJobId = "oldStopJobId" };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockProductDiscountService.Setup(m => m.UpdateAsync(It.IsAny<ProductDiscount>())).Returns(Task.FromResult(true));
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>())).Returns("1");
            mockJobClient.Setup(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>())).Returns("2");

            var time = new DateTime(2017, 12, 20, 12, 12, 12);
            EditProductDiscountViewModel editProductDiscountViewModel = new EditProductDiscountViewModel
            {
                DiscountStartTime = time,
                DiscountEndTime = time.AddDays(1),
                BasicProductDataViewModel = new BasicProductDataViewModel { ProductId = 1 }
            };

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.EditProductDiscount(editProductDiscountViewModel) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ProductsOnPromotion", result.RouteValues["action"]);
            Assert.AreEqual("Product", result.RouteValues["controller"]);
            mockProductDiscountService.Verify(m => m.UpdateAsync(It.IsAny<ProductDiscount>()), Times.Once);
            mockJobClient.Verify(m => m.Create(It.Is<Job>(job => job.Method.Name == "ActivateDiscount"), It.IsAny<ScheduledState>()), Times.Once);
            mockJobClient.Verify(m => m.Create(It.Is<Job>(job => job.Method.Name == "DisactivateDiscount"), It.IsAny<ScheduledState>()), Times.Once);
            mockJobClient.Verify(m => m.ChangeState("oldStartJobId", It.IsAny<DeletedState>(), null), Times.Once);
            mockJobClient.Verify(m => m.ChangeState("oldStopJobId", It.IsAny<DeletedState>(), null), Times.Once);
        }
        #endregion

        #region DeleteProductDiscount
        [Test]
        public async Task DeleteProductDiscount_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.DeleteProductDiscount() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteProductDiscount_returns_NotFound_if_productDiscount_is_null()
        {
            ProductDiscount nullProductDiscount = null;
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullProductDiscount);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);

            var result = await controller.DeleteProductDiscount(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeleteProductDiscount_removes_product_discount_and_hangfire_jobs_and_returns_redirect_url_as_json_result()
        {
            ProductDiscount productDiscount = new ProductDiscount { ProductId = 10, StartDiscountJobId = "startJobId", StopDiscountJobId = "stopJobId" };
            mockProductDiscountService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(productDiscount);
            mockProductDiscountService.Setup(m => m.DeleteAsync(It.IsAny<ProductDiscount>())).Returns(Task.FromResult(true));

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            var redirectUrl = "http://shopdemo.pl/products/promotions";
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>())).Returns(redirectUrl);

            ProductDiscountController controller = new ProductDiscountController(mockProductService.Object, mockProductDiscountService.Object, mapper, mockJobClient.Object);
            controller.Url = mockUrlHelper.Object;

            var result = await controller.DeleteProductDiscount(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(redirectUrl, jsonData.redirectUrl);

            mockProductDiscountService.Verify(m => m.DeleteAsync(It.IsAny<ProductDiscount>()),Times.Once);
            mockJobClient.Verify(m => m.ChangeState("startJobId", It.IsAny<DeletedState>(), null), Times.Once);
            mockJobClient.Verify(m => m.ChangeState("stopJobId", It.IsAny<DeletedState>(), null), Times.Once);
        }
        #endregion
    }
}