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
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class CartControllerTest
    {
        Mock<ICartService> mockCartService;
        Mock<IProductService> mockProductService;
        IMapper mapper;
        Mock<HttpContextBase> mockHttpContext;
        Mock<UrlHelper> mockUrlHelper;

        ShoppingCart cart;
        List<CartItem> cartItems;

        [SetUp]
        public void SetUp()
        {
            mockCartService = new Mock<ICartService>();
            mockProductService = new Mock<IProductService>();

            mockHttpContext = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            mockHttpRequest.Setup(m => m.UrlReferrer).Returns(new Uri("http://shopdemo.pl/produkty"));
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);

            mockUrlHelper = new Mock<UrlHelper>();
            mockUrlHelper.Setup(m => m.Action(It.IsAny<string>(), It.IsAny<string>())).Returns("/produkty");
            mockUrlHelper.Setup(m => m.Action("GetCart", "Cart")).Returns("/koszyk");

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ShoppingCartProfile());

            });
            mapper = config.CreateMapper();

            cartItems = new List<CartItem>();
            cartItems.Add(new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 2 });
            cartItems.Add(new CartItem { ProductId = 2, ProductCount = 4, ProductPrice = 5 });
            cartItems.Add(new CartItem { ProductId = 3, ProductCount = 2, ProductPrice = 7 });
            cartItems.Add(new CartItem { ProductId = 4, ProductCount = 6, ProductPrice = 10 });
            cartItems.Add(new CartItem { ProductId = 5, ProductCount = 1, ProductPrice = 4 });

            cart = new ShoppingCart();
            cart.CartItems = cartItems;
        }

        [Test]
        public void ShoppingCartLink_returns_CartLinkPartial_with_cart_items_count()
        {
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.ShoppingCartLink() as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("_CartLinkPartial", result.ViewName);
            Assert.AreEqual(5, (int)result.Model);
        }

        #region GetCart
        [Test]
        public void GetCart_assigns_tempdata_value_as_referrer_url_if_tempdata_is_not_null()
        {
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            string referrerUrl = "http://tempdataReferrerUrl.pl";
            var temp = new TempDataDictionary();
            temp.Add("urlReferrer", referrerUrl);

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            controller.TempData = temp;
            controller.Url = mockUrlHelper.Object;

            var result = controller.GetCart() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(referrerUrl, result.ViewBag.ReferrerUrl);
        }

        [Test]
        public void GetCart_returns_default_url_referrer_if_tempdata_and_Request_UrlReferrer_are_null()
        {
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            var temp = new TempDataDictionary();
            temp.Add("urlReferrer", null);

            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            Uri uri = null;
            mockHttpRequest.Setup(m => m.UrlReferrer).Returns(uri);
            mockHttpContext.Setup(m => m.Request).Returns(mockHttpRequest.Object);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            controller.TempData = temp;
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = controllerContext;

            var result = controller.GetCart() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("/produkty", result.ViewBag.ReferrerUrl);
        }

        [Test]
        public void GetCart_sets_Request_UrlReferrer_to_ViewBag_ReferrerUrl_if_Request_UrlReferrer_is_not_null_and_is_not_equal_to_cart_url()
        {
            mockCartService.Setup(m => m.GetCart()).Returns(cart);
            var temp = new TempDataDictionary();
            temp.Add("urlReferrer", null);

            var controllerContext = new ControllerContext(mockHttpContext.Object, new System.Web.Routing.RouteData(), new Mock<ControllerBase>().Object);

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            controller.TempData = temp;
            controller.Url = mockUrlHelper.Object;
            controller.ControllerContext = controllerContext;

            var result = controller.GetCart() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("http://shopdemo.pl/produkty", result.ViewBag.ReferrerUrl);
        }

        [Test]
        public void GetCart_returns_correct_cart_data()
        {
            mockCartService.Setup(m => m.GetCart()).Returns(cart);

            var temp = new TempDataDictionary();
            temp.Add("urlReferrer", "refUrl");

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            controller.TempData = temp;
            controller.Url = mockUrlHelper.Object;

            var result = controller.GetCart() as ViewResult;
            var cartModel = (ShoppingCartViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(5, cartModel.CartItems.Count);
            Assert.AreEqual(5, cartModel.TotalItemsCount);
            Assert.AreEqual(102, cartModel.TotalPrice);
        }
        #endregion

        #region AddToCart
        [Test]
        public async Task AddToCart_adds_new_product_with_normal_price()
        {
            Product product = new Product { ProductId = 6, Price = 100 };

            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(product));
            mockCartService.Setup(m => m.AddToCart(It.IsAny<CartItem>())).Callback((CartItem cartItem) => { cartItems.Add(cartItem); });

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            var result = await controller.AddToCart(6, It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockCartService.Verify(m => m.AddToCart(It.IsAny<CartItem>()), Times.Once);
            Assert.AreEqual(6, cartItems.Count);
            Assert.AreEqual(100, (int)cartItems.Where(p => p.ProductId == 6).Select(p => p.ProductPrice).Single());
        }

        [Test]
        public async Task AddToCart_adds_new_product_with_promotion_price_if_ProductDiscount_is_not_null_and_ProductDiscountStatus_is_DuringTime()
        {
            ProductDiscount productDiscount = new ProductDiscount { ProductId = 6, Status = ProductDiscountStatus.DuringTime, DiscountQuantity = 10 };
            Product product = new Product { ProductId = 6, Price = 100, ProductDiscount = productDiscount };

            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(product));
            mockCartService.Setup(m => m.AddToCart(It.IsAny<CartItem>())).Callback((CartItem cartItem) => { cartItems.Add(cartItem); });

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);
            var result = await controller.AddToCart(6, It.IsAny<int>()) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockCartService.Verify(m => m.AddToCart(It.IsAny<CartItem>()), Times.Once);
            Assert.AreEqual(6, cartItems.Count);
            Assert.AreEqual(90, (int)cartItems.Where(p => p.ProductId == 6).Select(p => p.ProductPrice).Single());
        }
        #endregion

        [Test]
        public void GetProductPromotionPrice_returns_correct_price()
        {
            decimal normalPrice = 100;
            int discountQuantity = 25;
            decimal expectedPromotionPrice = 75;

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.GetProductPromotionPrice(normalPrice, discountQuantity);

            Assert.AreEqual(expectedPromotionPrice, result);
        }

        [Test]
        public void EmptyCart_removes_all_cart_items()
        {
            mockCartService.Setup(m => m.EmptyCart()).Callback(() => { cartItems.Clear(); });

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.EmptyCart() as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsTrue(jsonData.success);
            Assert.Zero(cartItems.Count);
        }

        #region RemoveProductFromCart
        [Test]
        public void RemoveProductFromCart_returns_BadRequest_if_productId_is_not_provided()
        {
            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.RemoveProductFromCart(It.IsAny<string>()) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void RemoveProductFromCart_removes_product_form_cart()
        {
            mockCartService.Setup(m => m.RemoveFromCart(It.IsAny<int>())).Callback((int productId) =>
            {
                CartItem cartItem = cartItems.Where(p => p.ProductId == productId).Single();
                cartItems.Remove(cartItem);
            });

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.RemoveProductFromCart(It.IsAny<string>(), 1) as JsonResult;
            dynamic jsonData = result.Data;
            
            Assert.That(result, Is.Not.Null);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(4, cartItems.Count);
            Assert.AreEqual(0, cartItems.Where(p => p.ProductId == 1).Count());
        }
        #endregion

        #region UpdateProductCount
        [Test]
        public void UpdateProductCount_returns_BadRequest_if_productId_is_not_provided()
        {
            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.UpdateProductCount(It.IsAny<string>(), It.IsAny<int>()) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void UpdateProductCount_updates_product_count_in_cart()
        {
            mockCartService.Setup(m => m.UpdateCartProductCount(It.IsAny<int>(), It.IsAny<int>())).Callback((int productId, int productCount) =>
             {
                 CartItem cartItem = cartItems.Where(p => p.ProductId == productId).Single();
                 CartItem updatedCartItem = new CartItem
                 {
                     Product = cartItem.Product,
                     ProductId = cartItem.ProductId,
                     ProductPrice = cartItem.ProductPrice,
                     ProductCount = cartItem.ProductCount + productCount
                 };
                 cartItems.Remove(cartItem);
                 cartItems.Add(updatedCartItem);
             });

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.UpdateProductCount(It.IsAny<string>(), 2, 1) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.That(result, Is.Not.Null);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(5, cartItems.Count);
            Assert.AreEqual(4, cartItems.Where(p => p.ProductId == 1).Select(p=>p.ProductCount).Single());
        }
        #endregion

        #region GetProductCount
        [Test]
        public void GetProductCount_returns_BadRequest_if_productId_is_not_provided()
        {
            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.GetProductCount() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void GetProductCount_returns_correct_product_count()
        {
            mockCartService.Setup(m => m.GetProductCount(It.IsAny<int>())).Returns(5);

            CartController controller = new CartController(mockCartService.Object, mockProductService.Object, mapper);

            var result = controller.GetProductCount(1) as JsonResult;
            dynamic jsonData = result.Data;

            Assert.That(result, Is.Not.Null);
            Assert.IsTrue(jsonData.success);
            Assert.AreEqual(5, jsonData.productCount);
        }
        #endregion
    }
}
