using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class ProductAttributeServiceTest
    {
        ProductAttributeService service;
        FakeDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();
            service = new ProductAttributeService(context);
        }

        [Test]
        public async Task AddMultipleProductAttributesAsync_adds_attributes()
        {
            List<ProductAttribute> attributes = new List<ProductAttribute>();
            attributes.Add(new ProductAttribute { ProductAttributeId = 1, CategoryId = 1, Name = "Attrib1" });
            attributes.Add(new ProductAttribute { ProductAttributeId = 2, CategoryId = 1, Name = "Attrib2" });
            attributes.Add(new ProductAttribute { ProductAttributeId = 3, CategoryId = 1, Name = "Attrib3" });

            await service.AddMultipleProductAttributesAsync(attributes);

            var result = await service.GetAllAsync();

            CollectionAssert.AreEqual(attributes, result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public async Task ProductAttributeExistsAsync_returns_false_if_attribute_not_exixts()
        {
            bool result = await service.ProductAttributeExistsAsync(10);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task ProductAttributeExistsAsync_returns_true_if_attribute_exixts()
        {
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 10, CategoryId = 1, Name = "TestAttrib" });

            bool result = await service.ProductAttributeExistsAsync(10);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task UpdateAsync_updates_attribute_name_if_it_is_not_equal_to_existing_name()
        {
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 10, CategoryId = 1, Name = "TestAttrib" });

            ProductAttribute updatedAttribute = new ProductAttribute { ProductAttributeId = 10, CategoryId = 1, Name = "UpdatedName" };

            await service.UpdateAsync(updatedAttribute);

            var result = context.ProductAttributes.Find(10);

            Assert.AreEqual(updatedAttribute.Name, result.Name);
        }

        [Test]
        public async Task UpdateAsync_updates_DetailName_in_ProductDetails_if_new_attribute_name_is_not_equal_to_existing_name()
        {
            ProductDetail productDetail = new ProductDetail { ProductId = 1, DetailName = "TestAttrib" };
            context.ProductDetails.Add(productDetail);
            context.ProductCategory.Add(new ProductCategory { CategoryId = 1, ProductId = 1, Product = new Product { ProductId=1,  ProductDetails = new List<ProductDetail> { productDetail } } });
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 10, CategoryId = 1, Name = "TestAttrib" });

            ProductAttribute updatedAttribute = new ProductAttribute { ProductAttributeId = 10, CategoryId = 1, Name = "UpdatedName" };

            await service.UpdateAsync(updatedAttribute);

            var result = context.ProductDetails.Where(p => p.ProductId == 1).ToList();

            Assert.AreEqual(updatedAttribute.Name, result[0].DetailName);
        }
    }
}