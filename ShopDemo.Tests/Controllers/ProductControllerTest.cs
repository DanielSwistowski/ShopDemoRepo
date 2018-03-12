using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using PagedList;
using Service_Layer.Services;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.Controllers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class ProductControllerTest
    {
        Mock<IProductService> mockProductService;
        IMapper mapper;
        List<Product> productsList;
        Product product;
        List<Photo> productGallery;

        [SetUp]
        public void SetUp()
        {
            mockProductService = new Mock<IProductService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductProfile());

            });
            mapper = config.CreateMapper();

            Category category1 = new Category { CategoryId = 1, Name = "Category1" };
            Category category2 = new Category { CategoryId = 2, Name = "Category2" };
            Category category3 = new Category { CategoryId = 3, Name = "Category3" };

            List<ProductCategory> productCategories = new List<ProductCategory>();
            productCategories.Add(new ProductCategory { ProductCategoryId = 1, ProductId = 1, CategoryId = 1, Category = category1 });
            productCategories.Add(new ProductCategory { ProductCategoryId = 2, ProductId = 1, CategoryId = 2, Category = category2 });
            productCategories.Add(new ProductCategory { ProductCategoryId = 3, ProductId = 1, CategoryId = 3, Category = category3 });

            ProductDiscount productDiscount = new ProductDiscount { ProductId = 1, DiscountQuantity = 10, Status = ProductDiscountStatus.DuringTime };

            List<ProductDetail> productDetails = new List<ProductDetail>();
            productDetails.Add(new ProductDetail { ProductId = 1, DetailName = "DetailName1", DetailValue = "DetailValue1" });

            productGallery = new List<Photo>();
            productGallery.Add(new Photo { ProductId = 1, PhotoId = 1, PhotoPath = "photoPath" });

            product = new Product
            {
                ProductId = 1,
                Name = "Product1",
                Price = 100,
                Description = "Opis1",
                Quantity = 10,
                IsInOffer = true,
                CreatedAt = DateTime.Now,
                DeletedFromOfferDate = null,
                ProductCategory = productCategories,
                ProductDiscount = productDiscount,
                ProductDetails = productDetails,
                ProductGallery = productGallery
            };

            productsList = new List<Product>();
            productsList.Add(product);
            productsList.Add(new Product { ProductId = 2, Name = "Product2", Price = 10, Description = "Opis2", Quantity = 20, IsInOffer = true, CreatedAt = DateTime.Now, DeletedFromOfferDate = null });
            productsList.Add(new Product { ProductId = 3, Name = "Product3", Price = 10, Description = "Opis3", Quantity = 30, IsInOffer = true, CreatedAt = DateTime.Now, DeletedFromOfferDate = null });
            productsList.Add(new Product { ProductId = 4, Name = "Product4", Price = 10, Description = "Opis4", Quantity = 40, IsInOffer = true, CreatedAt = DateTime.Now, DeletedFromOfferDate = null });
            productsList.Add(new Product { ProductId = 5, Name = "Product5", Price = 10, Description = "Opis5", Quantity = 50, IsInOffer = true, CreatedAt = DateTime.Now, DeletedFromOfferDate = null });
        }

        [Test]
        public async Task Index_returns_paged_products()
        {
            Tuple<IEnumerable<Product>, int> productsTuple = Tuple.Create(productsList.AsEnumerable(), productsList.Count);

            mockProductService.Setup(m => m.PageAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<decimal?>(),
                It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<bool?>(), It.IsAny<bool>(),
                It.IsAny<Dictionary<string, IEnumerable<string>>>())).Returns(Task.FromResult(productsTuple));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.Index(null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>()) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(StaticPagedList<CustomerProductViewModel>), result.Model);
        }

        #region Details
        [Test]
        public async Task Details_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Details_returns_NotFound_if_product_is_null()
        {
            Product nullProduct = null;
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(nullProduct));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_return_NotFound_if_product_is_removed_form_offer()
        {
            Product productRemovedFormOffer = new Product { IsInOffer = false };
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(productRemovedFormOffer));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails(10) as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Details_returns_correct_product_data()
        {
            mockProductService.Setup(m => m.FindByPredicateAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>())).Returns(Task.FromResult(product));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.ProductDetails(10) as ViewResult;

            CustomerProductDetailsViewModel resultModel = (CustomerProductDetailsViewModel)result.Model;
            int[] categoriesArray = resultModel.ProductCategories.Select(c => c.CategoryId).ToArray();

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(1, resultModel.ProductId);
            Assert.AreEqual("Product1", resultModel.Name);
            Assert.AreEqual(10, resultModel.Quantity);
            Assert.AreEqual(100, resultModel.OldPrice);
            Assert.AreEqual(90, resultModel.NewPrice);
            Assert.IsTrue(resultModel.IsInPromotion);
            Assert.AreEqual(1, categoriesArray[0]);
            Assert.AreEqual(2, categoriesArray[1]);
            Assert.AreEqual(3, categoriesArray[2]);
            Assert.AreEqual("DetailName1", resultModel.ProductDetails.ToArray()[0].DetailName);
            Assert.AreEqual("DetailValue1", resultModel.ProductDetails.ToArray()[0].DetailValue);
            CollectionAssert.AreEqual(productGallery, resultModel.ProductGallery);
        }
        #endregion

        #region GetProductCount
        [Test]
        public async Task GetProductCount_returns_BadRequest_if_productId_is_not_provided()
        {
            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.GetProductCount() as HttpStatusCodeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task GetProductCount_returns_product_count_as_json_result()
        {
            mockProductService.Setup(m => m.GetProductCountAsync(It.IsAny<int>())).Returns(Task.FromResult(10));

            ProductController controller = new ProductController(mockProductService.Object, mapper);

            var result = await controller.GetProductCount(1) as JsonResult;

            dynamic jsonData = result.Data;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(true, jsonData.success);
            Assert.AreEqual(10, jsonData.productCount);
        }
        #endregion
    }
}
