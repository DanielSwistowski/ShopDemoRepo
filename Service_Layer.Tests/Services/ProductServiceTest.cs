using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class ProductServiceTest
    {
        ProductService service;
        FakeDbContext context;
        Mock<IPhotoFileManagement> mockPhotoFileManagement;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            context.Photos.Add(new Photo { PhotoId = 1, ProductId = 1, PhotoPath = "path1", PhotoThumbPath = "thumb1" });
            context.Photos.Add(new Photo { PhotoId = 2, ProductId = 1, PhotoPath = "path2", PhotoThumbPath = "thumb2" });

            List<ProductCategory> productCategory = new List<ProductCategory>();
            productCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 1 });
            productCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 2 });
            productCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 3 });

            List<ProductDetail> productDetails = new List<ProductDetail>();
            productDetails.Add(new ProductDetail { DetailName = "Detail1", DetailValue = "DetailValue1", ProductId = 1 });
            productDetails.Add(new ProductDetail { DetailName = "Detail2", DetailValue = "DetailValue2", ProductId = 1 });
            productDetails.Add(new ProductDetail { DetailName = "Detail3", DetailValue = "DetailValue3", ProductId = 1 });

            List<Photo> productGallery = new List<Photo>();
            productGallery.Add(new Photo { ProductId = 1, PhotoPath = "path1", PhotoThumbPath = "thumb1" });
            productGallery.Add(new Photo { ProductId = 1, PhotoPath = "path2", PhotoThumbPath = "thumb2" });
            productGallery.Add(new Photo { ProductId = 1, PhotoPath = "path3", PhotoThumbPath = "thumb3" });

            Product product = new Product
            {
                ProductId = 1,
                IsInOffer = true,
                DeletedFromOfferDate = null,
                Quantity = 10,
                Name = "TestProduct",
                Description = "Test description",
                Price = 20,
                CreatedAt = DateTime.Now,
                ProductCategory = productCategory,
                ProductDetails = productDetails,
                ProductGallery = productGallery
            };
            context.Products.Add(product);
            context.Products.Add(new Product { ProductId = 2, IsInOffer = false, DeletedFromOfferDate = DateTime.Now, Price = 5 });

            Product product3 = new Product { ProductId = 3, Name = "Product3", IsInOffer = true, Price = 10, ProductDiscount = new ProductDiscount(), ProductDetails = new List<ProductDetail> { new ProductDetail { DetailName = "TestDetail", DetailValue = "Value" } } };
            Product product4 = new Product { ProductId = 4, Name = "Product4", IsInOffer = true, Price = 10, ProductDetails = new List<ProductDetail> { new ProductDetail { DetailName = "TestDetail", DetailValue = "TestValue" } } };
            Product product5 = new Product { ProductId = 5, Name = "Product5", IsInOffer = false, Price = 40 };
            Product product6 = new Product { ProductId = 6, Name = "Product6", IsInOffer = false, Price = 20 };

            context.Products.Add(product3);
            context.Products.Add(product4);
            context.Products.Add(product5);
            context.Products.Add(product6);

            context.ProductCategory.Add(new ProductCategory { CategoryId = 2, ProductId = 3, Product = product3 });
            context.ProductCategory.Add(new ProductCategory { CategoryId = 2, ProductId = 4, Product = product4 });
            context.ProductCategory.Add(new ProductCategory { CategoryId = 2, ProductId = 5, Product = product5 });
            context.ProductCategory.Add(new ProductCategory { CategoryId = 2, ProductId = 6, Product = product6 });

            context.OrderDetails.Add(new OrderDetails { Product = product, ProductId = 1 });

            mockPhotoFileManagement = new Mock<IPhotoFileManagement>();

            service = new ProductService(context, mockPhotoFileManagement.Object);
        }

        #region PageAllAsync(int? pageNumber, int? pageSize, string searchProductByName, decimal? priceFrom, decimal? priceTo, int? categoryId = null, bool isInOffer = false, bool? isOnPromotion = null, bool includeProductRates = false, Dictionary<string, IEnumerable<string>> searchParameters = null)
        [Test]
        public async Task PageAllAsync_returns_products_form_selected_category_if_categoryId_is_not_null()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = 2;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(2, products.Count());
        }

        [Test]
        public async Task PageAllAsync_returns_all_products_if_categoryId_is_not_null()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(3, products.Count());
        }

        [Test]
        public async Task PageAllAsync_returns_products_removed_form_offer_if_isInOffer_is_false()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = false;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1.ToArray();

            Assert.AreEqual(3, products.Count());
            Assert.IsFalse(products[0].IsInOffer);
            Assert.IsFalse(products[1].IsInOffer);
            Assert.IsFalse(products[2].IsInOffer);
        }

        [Test]
        public async Task PageAllAsync_returns_products_existing_in_offer_if_isInOffer_is_true()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1.ToArray();

            Assert.AreEqual(3, products.Count());
            Assert.IsTrue(products[0].IsInOffer);
            Assert.IsTrue(products[1].IsInOffer);
            Assert.IsTrue(products[2].IsInOffer);
        }

        [Test]
        public async Task PageAllAsync_search_product_by_name()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "Product4";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual("Product4", products.Single().Name);
        }

        [Test]
        public async Task PageAllAsync_returns_products_during_promotion_if_isOnPromotion_is_true_and_ProductDiscount_is_not_null()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = true;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual("Product3", products.Single().Name);
            Assert.IsNotNull(products.Single().ProductDiscount);
        }

        [Test]
        public async Task PageAllAsync_search_products_by_price()
        {
            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = 10;
            decimal? priceTo = 30;
            int? categoryId = null;
            bool isInOffer = false;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual("Product6", products.Single().Name);
            Assert.That(products.Single().Price, Is.GreaterThan(10));
            Assert.That(products.Single().Price, Is.LessThan(30));
        }

        [Test]
        public async Task PageAllAsync_search_products_by_products_details()
        {
            Dictionary<string, IEnumerable<string>> testDetails = new Dictionary<string, IEnumerable<string>>();
            testDetails.Add("TestDetail", new List<string> { "TestValue" });

            int? pageNumber = null;
            int? pageSize = null;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = testDetails;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual("Product4", products.Single().Name);
            Assert.AreEqual("TestDetail", products.Single().ProductDetails.Single().DetailName);
            Assert.AreEqual("TestValue", products.Single().ProductDetails.Single().DetailValue);
        }

        [Test]
        public async Task PageAllAsync_returns_paged_products()
        {
            int? pageNumber = 1;
            int? pageSize = 2;
            string searchProductByName = "";
            decimal? priceFrom = null;
            decimal? priceTo = null;
            int? categoryId = null;
            bool isInOffer = true;
            bool? isOnPromotion = null;
            bool includeProductRates = false;
            Dictionary<string, IEnumerable<string>> searchParameters = null;

            var productsTuple = await service.PageAllAsync(pageNumber, pageSize, searchProductByName, priceFrom, priceTo, categoryId, isInOffer, isOnPromotion, includeProductRates, searchParameters);

            var products = productsTuple.Item1;
            var totalProductsCount = productsTuple.Item2;

            Assert.AreEqual(3, totalProductsCount);
            Assert.AreEqual(2, products.Count());
        }
        #endregion

        [Test]
        public async Task DeleteAsync_removes_product_and_gallery()
        {
            Product productToDelete = context.Products.Find(1);

            await service.DeleteAsync(productToDelete);

            Assert.IsNull(context.Products.Find(1));
        }

        [Test]
        public async Task AddProductToOffer_sets_product_properties_IsInOffer_to_true_and_DeletedFromOfferDate_to_null()
        {
            Product product = context.Products.Find(2);

            await service.AddProductToOffer(product);

            var result = context.Products.Find(2);
            Assert.IsTrue(result.IsInOffer);
            Assert.IsNull(result.DeletedFromOfferDate);
        }

        [Test]
        public async Task RemoveProductFromOffer_sets_product_properties_IsInOffer_to_false_and_DeletedFromOfferDate_to_actual_datetime()
        {
            Product product = context.Products.Find(1);

            await service.RemoveProductFromOffer(product);

            var result = context.Products.Find(1);
            Assert.IsFalse(result.IsInOffer);
            Assert.IsNotNull(result.DeletedFromOfferDate);
        }

        [Test]
        public async Task ProductExistsInOrders_returns_true_if_any_order_detail_contains_selected_product()
        {
            int productId = 1;

            var result = await service.ProductExistsInOrders(productId);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task ProductExistsInOrders_returns_false_if_selected_product_not_exists_in_any_order_detail()
        {
            int productId = 2;

            var result = await service.ProductExistsInOrders(productId);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetProductCountAsync_returns_product_quantity_if_selected_product_is_not_null()
        {
            int expectedProductQuantity = 10;
            int productId = 1;
            var result = await service.GetProductCountAsync(productId);

            Assert.AreEqual(expectedProductQuantity, result);
        }

        [Test]
        public async Task GetProductCountAsync_returns_0_if_selected_product_is_null()
        {
            int expectedProductQuantity = 0;
            int productId = 10;
            var result = await service.GetProductCountAsync(productId);

            Assert.AreEqual(expectedProductQuantity, result);
        }

        #region UpdateAsync
        [Test]
        public async Task UpdateAsync_updates_product_data()
        {
            Product updatedProduct = new Product { ProductId = 1, IsInOffer = true, DeletedFromOfferDate = null, Quantity = 4, Name = "UpdatedName", Description = "UpdatedDescription", Price = 40, CreatedAt = DateTime.Now };

            await service.UpdateAsync(updatedProduct);

            var result = context.Products.Find(1);

            Assert.AreEqual(4, result.Quantity);
            Assert.AreEqual("UpdatedName", result.Name);
            Assert.AreEqual("UpdatedDescription", result.Description);
            Assert.AreEqual(40, result.Price);
        }

        [Test]
        public async Task UpdateAsync_updates_product_category()
        {
            List<ProductCategory> updatedProductCategory = new List<ProductCategory>();
            updatedProductCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 4 });
            updatedProductCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 5 });
            updatedProductCategory.Add(new ProductCategory { ProductId = 1, CategoryId = 6 });

            Product updatedProduct = new Product { ProductId = 1, ProductCategory = updatedProductCategory };

            await service.UpdateAsync(updatedProduct);

            var result = context.ProductCategory.Where(p => p.ProductId == 1).ToArray();

            Assert.AreEqual(updatedProductCategory[0].CategoryId, result[0].CategoryId);
            Assert.AreEqual(updatedProductCategory[1].CategoryId, result[1].CategoryId);
            Assert.AreEqual(updatedProductCategory[2].CategoryId, result[2].CategoryId);
        }

        [Test]
        public async Task UpdateAsync_updates_product_details()
        {
            List<ProductDetail> updatedProductDetails = new List<ProductDetail>();
            updatedProductDetails.Add(new ProductDetail { DetailName = "NewDetail", DetailValue = "NewDetailValue", ProductId = 1 });
            updatedProductDetails.Add(new ProductDetail { DetailName = "Detail2", DetailValue = "DetailValue22", ProductId = 1 });
            updatedProductDetails.Add(new ProductDetail { DetailName = "Detail3", DetailValue = "DetailValue33", ProductId = 1 });

            Product updatedProduct = new Product { ProductId = 1, ProductDetails = updatedProductDetails };

            await service.UpdateAsync(updatedProduct);

            var result = context.ProductDetails.Where(p => p.ProductId == 1).ToArray();

            Assert.IsTrue(result.Count() == 3);
            Assert.AreEqual(updatedProductDetails[0].DetailName, result[0].DetailName);
            Assert.AreEqual(updatedProductDetails[0].DetailValue, result[0].DetailValue);
            Assert.AreEqual(updatedProductDetails[1].DetailName, result[1].DetailName);
            Assert.AreEqual(updatedProductDetails[1].DetailValue, result[1].DetailValue);
            Assert.AreEqual(updatedProductDetails[2].DetailName, result[2].DetailName);
            Assert.AreEqual(updatedProductDetails[2].DetailValue, result[2].DetailValue);
        }

        [Test]
        public async Task UpdateAsync_updates_product_gallery()
        {
            List<Photo> updatedProductGallery = new List<Photo>();
            updatedProductGallery.Add(new Photo { ProductId = 1, PhotoPath = "newPhotoPath", PhotoThumbPath = "newPhotoThumb" });
            updatedProductGallery.Add(new Photo { ProductId = 1, PhotoPath = "path2", PhotoThumbPath = "thumb2" });
            updatedProductGallery.Add(new Photo { ProductId = 1, PhotoPath = "path3", PhotoThumbPath = "thumb3" });

            Product updatedProduct = new Product { ProductId = 1, ProductGallery = updatedProductGallery };

            await service.UpdateAsync(updatedProduct);

            var result = context.Photos.Where(p => p.ProductId == 1).ToArray();

            Assert.IsTrue(result.Count() == 3);
            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoPath).Contains("newPhotoPath"));
            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoPath).Contains("path2"));
            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoPath).Contains("path3"));

            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoThumbPath).Contains("newPhotoThumb"));
            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoThumbPath).Contains("thumb2"));
            Assert.IsTrue(updatedProductGallery.Select(p => p.PhotoThumbPath).Contains("thumb3"));

            mockPhotoFileManagement.Verify(m => m.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
        #endregion

        [Test]
        public async Task ActualizeProductQuantityAsync_updates_product_count()
        {
            int productId = 1;
            int quantity = 10;

            await service.ActualizeProductQuantityAsync(productId, quantity);

            var product = context.Products.Find(1);

            Assert.AreEqual(20, product.Quantity);
        }

        [Test]
        public async Task GetProductNameByProductIdAsync_returns_product_name()
        {
            string productName = await service.GetProductNameByProductIdAsync(1);

            Assert.AreEqual("TestProduct", productName);
        }
    }
}