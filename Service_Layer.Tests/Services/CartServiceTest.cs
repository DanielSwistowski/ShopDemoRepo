using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using System.Collections.Generic;
using System.Web;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class CartServiceTest
    {
        CartService service;

        [SetUp]
        public void SetUp()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();
            Mock<HttpSessionStateBase> mockSession = new Mock<HttpSessionStateBase>();
            mockHttpContextBase.Setup(m => m.Session).Returns(mockSession.Object);

            service = new CartService(mockHttpContextBase.Object);
        }

        #region AddToCart
        [Test]
        public void AddToCart_create_new_cart_items_list_and_adds_item_to_cart_if_CartItems_property_is_null()
        {
            CartItem item = new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 };

            service.AddToCart(item);

            var result = service.GetCart();

            Assert.IsTrue(result.CartItems.Contains(item));
        }

        [Test]
        public void AddToCart_adds_new_item_to_cart_if_item_not_exists_in_CartItems_collection()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);


            CartItem item = new CartItem { ProductId = 2, ProductCount = 5, ProductPrice = 10 };

            service.AddToCart(item);

            var result = service.GetCart();

            Assert.IsTrue(result.TotalItemsCount == 2);
            Assert.IsTrue(result.CartItems.Contains(item));
            Assert.IsTrue(result.CartItems[0].ProductCount == 2);
            Assert.IsTrue(result.CartItems[1].ProductCount == 5);
        }

        [Test]
        public void AddToCart_increases_item_count_if_item_exists_in_CartItems_collection()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);


            CartItem item = new CartItem { ProductId = 1, ProductCount = 5, ProductPrice = 10 };

            service.AddToCart(item);

            var result = service.GetCart();

            Assert.IsTrue(result.TotalItemsCount == 1);
            Assert.IsTrue(result.CartItems[0].ProductCount == 7);
        }
        #endregion

        [Test]
        public void RemoveFromCart_removes_item_form_cart()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);

            int productId = 1;
            service.RemoveFromCart(productId);

            var result = service.GetCart();

            Assert.IsTrue(result.TotalItemsCount == 0);
        }

        #region UpdateCartProductCount
        [Test]
        public void UpdateCartProductCount_updates_product_count_if_cart_item_is_not_null_and_productCount_is_not_equal_to_0()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);

            int productId = 1;
            int productCount = 5;
            service.UpdateCartProductCount(productId, productCount);

            var result = service.GetCart();

            Assert.IsTrue(result.TotalItemsCount == 1);
            Assert.AreEqual(productCount, result.CartItems[0].ProductCount);
        }

        [Test]
        public void UpdateCartProductCount_remove_product_from_cart_if_cart_item_is_not_null_and_productCount_is_equal_to_0()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);

            int productId = 1;
            int productCount = 0;
            service.UpdateCartProductCount(productId, productCount);

            var result = service.GetCart();

            Assert.IsTrue(result.TotalItemsCount == 0);
        }
        #endregion

        [Test]
        public void EmptyCart_clear_user_session_if_cart_is_not_null()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();
            Mock<HttpSessionStateBase> mockSession = new Mock<HttpSessionStateBase>();
            mockSession.Setup(m => m.Clear());
            mockHttpContextBase.Setup(m => m.Session).Returns(mockSession.Object);

            CartService service = new CartService(mockHttpContextBase.Object);

            service.EmptyCart();
            
            mockSession.Verify(m => m.Clear(), Times.Once);
        }

        [Test]
        public void GetProductCount_returns_product_count_if_CartItems_property_is_not_null_and_selected_product_is_not_null()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            cart.CartItems = new List<CartItem> { new CartItem { ProductId = 1, ProductCount = 2, ProductPrice = 20 } };
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);

            int productId = 1;
            var result = service.GetProductCount(productId);
            
            Assert.AreEqual(2, result);
        }

        [Test]
        public void GetProductCount_returns_0_if_CartItems_property_is_not_null_but_selected_product_is_null()
        {
            Mock<HttpContextBase> mockHttpContextBase = new Mock<HttpContextBase>();

            ShoppingCart cart = new ShoppingCart();
            mockHttpContextBase.Setup(m => m.Session["CartId"]).Returns(cart);

            CartService service = new CartService(mockHttpContextBase.Object);

            int productId = 1;
            var result = service.GetProductCount(productId);

            Assert.AreEqual(0, result);
        }
    }
}