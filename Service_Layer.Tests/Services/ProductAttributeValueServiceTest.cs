using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class ProductAttributeValueServiceTest
    {
        ProductAttributeValueService service;
        FakeDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            service = new ProductAttributeValueService(context);
        }

        [Test]
        public async Task AddMultipleProductAttributeValuesAsync_adds_attribute_values()
        {
            List<ProductAttributeValue> attributeValues = new List<ProductAttributeValue>();
            attributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 1, ProductAttributeId = 1, AttributeValue = "Value1" });
            attributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 2, ProductAttributeId = 1, AttributeValue = "Value2" });
            attributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 3, ProductAttributeId = 1, AttributeValue = "Value3" });

            await service.AddMultipleProductAttributeValuesAsync(attributeValues);

            var result = context.ProductAttributeValues.ToList();

            CollectionAssert.AreEqual(attributeValues, result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public async Task GetProductAttributeValuesByCategoryIdAndAttributeNameAsync_returns_attribute_values()
        {
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 1, CategoryId = 1, Name = "Attrib1" });
            context.ProductAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 1, ProductAttributeId = 1, AttributeValue = "Value1" });
            context.ProductAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 2, ProductAttributeId = 1, AttributeValue = "Value2" });
            context.ProductAttributeValues.Add(new ProductAttributeValue { ProductAttributeValueId = 3, ProductAttributeId = 20, AttributeValue = "Value3" });

            var result = await service.GetProductAttributeValuesByCategoryIdAndAttributeNameAsync(1, "Attrib1");
            var resultArray = result.ToArray();

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Value1", resultArray[0].AttributeValue);
            Assert.AreEqual("Value2", resultArray[1].AttributeValue);
            Assert.IsFalse(result.Where(a => a.AttributeValue == "Value3").Any());
        }
    }
}
