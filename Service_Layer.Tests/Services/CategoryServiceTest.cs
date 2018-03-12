using DataAccessLayer;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class CategoryServiceTest
    {
        CategoryService service;

        [SetUp]
        public void SetUp()
        {
            FakeDbContext context = new FakeDbContext();

            context.ProductCategory.Add(new ProductCategory { ProductCategoryId = 1, CategoryId = 1, ProductId = 1 });
            context.ProductCategory.Add(new ProductCategory { ProductCategoryId = 2, CategoryId = 2, ProductId = 2 });
            context.ProductCategory.Add(new ProductCategory { ProductCategoryId = 3, CategoryId = 5, ProductId = 2 });

            context.Categories.Add(new Category { CategoryId = 1, ParentCategoryId = null, Name = "Category1" });
            context.Categories.Add(new Category { CategoryId = 2, ParentCategoryId = null, Name = "Category2" });
            context.Categories.Add(new Category { CategoryId = 3, ParentCategoryId = 1, Name = "Category3" });
            context.Categories.Add(new Category { CategoryId = 4, ParentCategoryId = 1, Name = "Category4" });
            context.Categories.Add(new Category { CategoryId = 5, ParentCategoryId = 2, Name = "Category5" });

            context.Products.Add(new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10, IsInOffer = true, Quantity = 2, DeletedFromOfferDate = null, CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 2, Name = "TestName", Description = "Description2", Price = 20, IsInOffer = true, Quantity = 4, DeletedFromOfferDate = null, CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 3, Name = "Product2", Description = "Description3", Price = 30, IsInOffer = true, Quantity = 6, DeletedFromOfferDate = null, CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 4, Name = "Product3", Description = "Description4", Price = 40, IsInOffer = false, Quantity = 8, DeletedFromOfferDate = new DateTime(2017, 10, 24, 10, 25, 42), CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 5, Name = "Product4", Description = "Description5", Price = 50, IsInOffer = false, Quantity = 10, DeletedFromOfferDate = new DateTime(2017, 11, 19, 12, 18, 42), CreatedAt = DateTime.Now });
            
            service = new CategoryService(context);
        }

        [Test]
        public async Task CategoryContainsProductsAsync_returns_true_if_category_contains_products()
        {
            var result = await service.CategoryContainsProductsAsync(1);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task CategoryContainsProductsAsync_returns_false_if_category_not_contains_products()
        {
            var result = await service.CategoryContainsProductsAsync(20);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task CategoryExistsAsParentAsync_returns_true_if_category_exist_as_parent_category()
        {
            var result = await service.CategoryExistsAsParentAsync(2);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task CategoryExistsAsParentAsync_returns_false_if_category_not_exist_as_parent_category()
        {
            var result = await service.CategoryExistsAsParentAsync(4);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task FindCategoryParentIdAsync_returns_correct_category_parentId()
        {
            var result = await service.FindCategoryParentIdAsync(4);

            Assert.That(result == 1);
        }

        [Test]
        public async Task FindProductCategoriesIdsByProductNameAsync_returns_categories_ids()
        {
            var result = await service.FindProductCategoriesIdsByProductNameAsync("Test");

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(2, result.ToArray()[0]);
            Assert.AreEqual(5, result.ToArray()[1]);
        }


        [Test]
        public async Task AddAsync_adds_new_category()
        {
            Category category = new Category { CategoryId=19, Name="TestAdd", ParentCategoryId=2 };

            await service.AddAsync(category);

            var result = await service.GetAllAsync();

            Assert.AreEqual(6, result.Count());
            Assert.IsTrue(result.Contains(category));
        }

    }
}