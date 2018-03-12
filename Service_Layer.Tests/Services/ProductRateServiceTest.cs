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
    public class ProductRateServiceTest
    {
        ProductRateService service;

        [SetUp]
        public void SetUp()
        {
            FakeDbContext context = new FakeDbContext();

            context.ProductRates.Add(new ProductRate { ProductRateId = 1, ProductId = 1, Rate = 2, Comment = "comment", CreatedAt = DateTime.Now, NickName = "John" });

            service = new ProductRateService(context);
        }

        [Test]
        public async Task NickNameIsAvailableForCurrentProduct_returns_true_if_nick_is_available()
        {
            int productId = 1;
            string nickName = "Jan";

            var result = await service.NickNameIsAvailableForCurrentProduct(productId, nickName);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task NickNameIsAvailableForCurrentProduct_returns_false_if_nick_is_not_available()
        {
            int productId = 1;
            string nickName = "John";

            var result = await service.NickNameIsAvailableForCurrentProduct(productId, nickName);

            Assert.IsFalse(result);
        }
    }
}
