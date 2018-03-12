using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class SearchFilterServiceTest
    {
        SearchFilterService service;
        FakeDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            context.SearchFilters.Add(new SearchFilter { ProductAttributeId = 1, CategoryId = 1, FilterType = FilterType.CheckboxList });

            service = new SearchFilterService(context);
        }

        [Test]
        public async Task SearchFilterExistsAsync_returns_true_if_filter_exists()
        {
            int productAttributeId = 1;

            var result = await service.SearchFilterExistsAsync(productAttributeId);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task SearchFilterExistsAsync_returns_false_if_filter_not_exists()
        {
            int productAttributeId = 2;

            var result = await service.SearchFilterExistsAsync(productAttributeId);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task RemoveUnexistingParametesFromFiltrAsync_removes_product_attributes_which_not_exists_for_products_from_selected_category()
        {
            List<ProductAttributeValue> atributeValues1 = new List<ProductAttributeValue>();
            atributeValues1.Add(new ProductAttributeValue { ProductAttributeId = 1, AttributeValue = "1" });
            atributeValues1.Add(new ProductAttributeValue { ProductAttributeId = 1, AttributeValue = "2" });
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 1, Name = "Attribute1", ProductAttributeValues = atributeValues1, CategoryId = 1, SearchFilter = new SearchFilter() });

            List<ProductAttributeValue> atributeValues2 = new List<ProductAttributeValue>();
            atributeValues2.Add(new ProductAttributeValue { ProductAttributeId = 2, AttributeValue = "5" });
            atributeValues2.Add(new ProductAttributeValue { ProductAttributeId = 2, AttributeValue = "6" });
            context.ProductAttributes.Add(new ProductAttribute { ProductAttributeId = 2, Name = "Attribute2", ProductAttributeValues = atributeValues2, CategoryId = 1, SearchFilter = new SearchFilter() });

            Dictionary<string, IEnumerable<string>> filtr = new Dictionary<string, IEnumerable<string>>();
            filtr.Add("attribute1", new List<string> { "1", "2", "3", "4" });
            filtr.Add("attribute2", new List<string> { "5", "6" });
            filtr.Add("attribute4", new List<string> { "7", "8" });

            var result = await service.RemoveUnexistingParametesFromFiltrAsync(1, filtr);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new List<string> { "1", "2" }, result["attribute1"]);
            Assert.IsFalse(result["attribute1"].Contains("3"));
            Assert.IsFalse(result["attribute1"].Contains("4"));
            Assert.AreEqual(new List<string> { "5", "6" }, result["attribute2"]);
            Assert.IsFalse(result.ContainsKey("attribute4"));
        }
    }
}