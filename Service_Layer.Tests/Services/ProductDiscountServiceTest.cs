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
    public class ProductDiscountServiceTest
    {
        ProductDiscountService service;
        FakeDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            context.Products.Add(new Product { ProductId = 1, Price = 100 });

            context.ProductDiscounts.Add(new ProductDiscount { ProductId = 1, Status = ProductDiscountStatus.WaitingForStart });

            context.ProductDiscounts.Add(new ProductDiscount { ProductId = 2, Status = ProductDiscountStatus.DuringTime });

            service = new ProductDiscountService(context);
        }

        [Test]
        public async Task CalculateNewProductPriceAsync_returns_new_product_price()
        {
            int productId = 1;
            int discountQuantity = 25;
            var result = await service.CalculateNewProductPriceAsync(productId, discountQuantity);

            Assert.AreEqual(75, result);
        }

        [Test]
        public void ActivateDiscount_sets_discount_status_to_DuringTime_if_selected_discount_is_not_null_and_discount_status_is_WaitingForStart()
        {
            int productId = 1;
            service.ActivateDiscount(productId);

            var result = context.ProductDiscounts.Find(productId);

            Assert.AreEqual(ProductDiscountStatus.DuringTime, result.Status);
        }

        [Test]
        public void DisactivateDiscount_sets_discount_status_to_Ended_if_selected_discount_is_not_null_and_discount_status_is_DuringTime()
        {
            int productId = 2;
            service.DisactivateDiscount(productId);

            var result = context.ProductDiscounts.Find(productId);

            Assert.AreEqual(ProductDiscountStatus.Ended, result.Status);
        }
    }
}
