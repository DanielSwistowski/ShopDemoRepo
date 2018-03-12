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
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class ProductControllerTest
    {
        Mock<IProductService> mockProductService;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockProductService = new Mock<IProductService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductProfile());

            });
            mapper = config.CreateMapper();
        }

        #region ProductDetails
        [Test]
        public async Task ProductDetails_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails(It.IsAny<string>()) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ProductDetails_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullProduct);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails(It.IsAny<string>(), 10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task ProductDetails_set_value_Index_into_viewbag_PreviousActionName_if_tempdata_actionName_is_null()
        {
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Product());
            var temp = new TempDataDictionary();
            temp.Add("actionName", null);

            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.TempData = temp;

            var result = await controller.ProductDetails(It.IsAny<string>(), 10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewBag.PreviousActionName);
        }

        [Test]
        public async Task ProductDetails_set_value_from_tempdata_actionName_into_viewbag_PreviousActionName_if_tempdata_actionName_is_not_null()
        {
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Product());
            var temp = new TempDataDictionary();
            temp.Add("actionName", "ActionName");

            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.TempData = temp;

            var result = await controller.ProductDetails(It.IsAny<string>(), 10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ActionName", result.ViewBag.PreviousActionName);
        }
        #endregion

        #region AddProduct
        [Test]
        public async Task AddProduct_returns_model_state_error_if_model_state_is_invalid()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.AddProduct(It.IsAny<AddProductViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task AddProduct_adds_new_product_and_redirects_to_Index_action_if_model_state_is_valid()
        {
            mockProductService.Setup(m => m.AddAsync(It.IsAny<Product>())).Returns(Task.FromResult(true));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.AddProduct(new AddProductViewModel { SelectedCategories = new List<int>() }) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockProductService.Verify(m => m.AddAsync(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region EditProduct
        [Test]
        public async Task EditProduct_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.EditProduct() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task EditProduct_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullProduct);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.EditProduct(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task EditProduct_set_Request_UrlReferrer_value_to_model_returnUrl_if_request_urlReferrer_is_not_null()
        {
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Product());

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            var url = new Uri("http://shopdemo.pl/products");
            mockRequest.Setup(m => m.UrlReferrer).Returns(url);
            mockHttpContext.Setup(m => m.Request).Returns(mockRequest.Object);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.ControllerContext = controllerContext;

            var result = await controller.EditProduct(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(url, ((EditProductViewModel)result.Model).ReturnUrl);
        }

        [Test]
        public async Task EditProduct_set_Index_action_url_value_to_model_returnUrl_if_request_urlReferrer_is_not_null()
        {
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(new Product());

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            Uri url = null;
            mockRequest.Setup(m => m.UrlReferrer).Returns(url);
            mockHttpContext.Setup(m => m.Request).Returns(mockRequest.Object);

            Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();
            var returnUrl = "http://shopdemo.pl/products";
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>())).Returns(returnUrl);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.ControllerContext = controllerContext;
            controller.Url = mockUrlHelper.Object;

            var result = await controller.EditProduct(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(returnUrl, ((EditProductViewModel)result.Model).ReturnUrl);
        }

        [Test]
        public async Task EditProduct_returns_model_state_error_if_model_state_is_not_valid()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.EditProduct(It.IsAny<EditProductViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task EditProduct_updates_product_data_and_redirects_to_model_returnUrl_if_model_state_is_valid()
        {
            var returnUrl = "http://shopdemo.pl/products";
            mockProductService.Setup(m => m.UpdateAsync(It.IsAny<Product>())).Returns(Task.FromResult(true));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.EditProduct(new EditProductViewModel { ReturnUrl = returnUrl, SelectedCategories = new List<int>() }) as RedirectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            mockProductService.Verify(m => m.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region DeleteProduct
        [Test]
        public async Task DeleteProduct_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProduct() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteProduct_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullProduct);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProduct(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeleteProduct_returns_productId_equals_to_0_as_json_result_if_product_exists_in_orders()
        {
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockProductService.Setup(m => m.ProductExistsInOrders(It.IsAny<int>())).ReturnsAsync(true);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProduct(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.AreEqual(0, jsonData);
        }

        [Test]
        public async Task DeleteProduct_removes_product_and_returns_removed_productId_as_json_result_if_product_not_exists_in_orders()
        {
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockProductService.Setup(m => m.ProductExistsInOrders(It.IsAny<int>())).ReturnsAsync(false);
            mockProductService.Setup(m => m.DeleteAsync(It.IsAny<Product>())).Returns(Task.FromResult(true));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProduct(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.AreEqual(10, jsonData);
            mockProductService.Verify(m => m.DeleteAsync(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region DeleteProductFromOffer
        [Test]
        public async Task DeleteProductFromOffer_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProductFromOffer() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteProductFromOffer_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(nullProduct);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProductFromOffer(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeleteProductFromOffer_returns_error_message_as_json_result_if_ProductDiscount_is_not_null_and_discount_Status_is_not_Ended()
        {
            ProductDiscount pD = new ProductDiscount() { Status = ProductDiscountStatus.DuringTime };
            Product p = new Product() { ProductDiscount = pD };
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(p);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProductFromOffer(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public async Task DeleteProductFromOffer_removes_product_form_offer_and_returns_removed_productId_as_json_result()
        {
            ProductDiscount pD = new ProductDiscount() { Status = ProductDiscountStatus.Ended };
            Product p = new Product() { ProductDiscount = pD };
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).ReturnsAsync(p);
            mockProductService.Setup(m => m.RemoveProductFromOffer(It.IsAny<Product>())).Returns(Task.FromResult(true));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.DeleteProductFromOffer(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(10, jsonData.productId);
            mockProductService.Verify(m => m.RemoveProductFromOffer(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region AddProductToOffer
        [Test]
        public async Task AddProductToOffer_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.AddProductToOffer() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task AddProductToOffer_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullProduct);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.AddProductToOffer(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task AddProductToOffer_adds_product_to_offer_and_returns_added_productId_as_json_result()
        {
            mockProductService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product());
            mockProductService.Setup(m => m.AddProductToOffer(It.IsAny<Product>())).Returns(Task.FromResult(true));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.AddProductToOffer(10) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.AreEqual(10, jsonData);
            mockProductService.Verify(m => m.AddProductToOffer(It.IsAny<Product>()), Times.Once);
        }
        #endregion

        #region ActualizeProductQuantity
        [Test]
        public async Task ActualizeProductQuantity_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ActualizeProductQuantity(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ActualizeProductQuantity_returns_BadRequest_if_quantity_is_less_than_zero()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ActualizeProductQuantity(-1, 10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ActualizeProductQuantity_updates_product_count_and_returns_new_product_count_as_json_result()
        {
            mockProductService.Setup(m => m.ActualizeProductQuantityAsync(It.IsAny<int>(),It.IsAny<int>())).ReturnsAsync(10);

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ActualizeProductQuantity(10, 10) as JsonResult;
            dynamic jsonData = result.Data;
            
            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(10, jsonData.updatedQuantity);
        }
        #endregion
    }
}