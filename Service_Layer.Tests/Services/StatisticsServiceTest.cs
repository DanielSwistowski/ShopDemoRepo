using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class StatisticsServiceTest
    {
        StatisticsService service;
        FakeDbContext context;

        Product product1;
        Product product2;
        Product product3;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            product1 = new Product { ProductId = 1, Name = "Product1" };
            product2 = new Product { ProductId = 2, Name = "Product2" };
            product3 = new Product { ProductId = 3, Name = "Product3" };
            Product product4 = new Product { ProductId = 4, Name = "Product4" };
            Product product5 = new Product { ProductId = 5, Name = "Product5" };

            ProductRate productRate1 = new ProductRate { ProductId = 1, Rate = 4, Product = product1 };
            ProductRate productRate2 = new ProductRate { ProductId = 1, Rate = 4, Product = product1 };
            ProductRate productRate3 = new ProductRate { ProductId = 2, Rate = 5, Product = product2 };
            ProductRate productRate4 = new ProductRate { ProductId = 2, Rate = 4, Product = product2 };
            ProductRate productRate5 = new ProductRate { ProductId = 3, Rate = 4, Product = product3 };
            context.ProductRates.Add(productRate1);
            context.ProductRates.Add(productRate2);
            context.ProductRates.Add(productRate3);
            context.ProductRates.Add(productRate4);
            context.ProductRates.Add(productRate5);

            OrderDetails orderDetails1 = new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, Product = product1 };
            OrderDetails orderDetails2 = new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, Product = product2 };
            OrderDetails orderDetails3 = new OrderDetails { OrderDetailsId = 3, OrderId = 2, ProductId = 1, Product = product1 };
            OrderDetails orderDetails4 = new OrderDetails { OrderDetailsId = 4, OrderId = 2, ProductId = 3, Product = product3 };
            OrderDetails orderDetails5 = new OrderDetails { OrderDetailsId = 5, OrderId = 3, ProductId = 4, Product = product4 };
            OrderDetails orderDetails6 = new OrderDetails { OrderDetailsId = 6, OrderId = 3, ProductId = 5, Product = product5 };
            OrderDetails orderDetails7 = new OrderDetails { OrderDetailsId = 7, OrderId = 4, ProductId = 4, Product = product4 };
            OrderDetails orderDetails8 = new OrderDetails { OrderDetailsId = 8, OrderId = 4, ProductId = 1, Product = product1 };
            context.OrderDetails.Add(orderDetails1);
            context.OrderDetails.Add(orderDetails2);
            context.OrderDetails.Add(orderDetails3);
            context.OrderDetails.Add(orderDetails4);
            context.OrderDetails.Add(orderDetails5);
            context.OrderDetails.Add(orderDetails6);
            context.OrderDetails.Add(orderDetails7);
            context.OrderDetails.Add(orderDetails8);

            product1.OrderDetails = new List<OrderDetails> { orderDetails1, orderDetails3, orderDetails8 };
            product2.OrderDetails = new List<OrderDetails> { orderDetails2 };
            product3.OrderDetails = new List<OrderDetails> { orderDetails4 };
            product4.OrderDetails = new List<OrderDetails> { orderDetails5, orderDetails7 };
            product5.OrderDetails = new List<OrderDetails> { orderDetails6 };

            ApplicationUser user1 = new ApplicationUser { FirstName = "User1" };
            ApplicationUser user2 = new ApplicationUser { FirstName = "User2" };
            ApplicationUser user3 = new ApplicationUser { FirstName = "User3" };

            Order order1 = new Order { OrderId = 1, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), OrderDetails = new List<OrderDetails> { orderDetails1, orderDetails2 }, User = user1, TotalAmount = 456 };
            Order order2 = new Order { OrderId = 2, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 02, 22, 15, 22, 08), OrderDetails = new List<OrderDetails> { orderDetails3, orderDetails4 }, User = user2, TotalAmount = 2684 };
            Order order3 = new Order { OrderId = 3, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), OrderDetails = new List<OrderDetails> { orderDetails5, orderDetails6 }, User = user3, TotalAmount = 126 };
            Order order4 = new Order { OrderId = 4, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), OrderDetails = new List<OrderDetails> { orderDetails7, orderDetails8 }, User = user1, TotalAmount = 728 };

            context.Orders.Add(order1);
            context.Orders.Add(order2);
            context.Orders.Add(order3);
            context.Orders.Add(order4);

            service = new StatisticsService(context);
        }

        [Test]
        public async Task GetMostSoldProductsAsync_returns_selected_amount_of_products()
        {
            int take = 2;

            var result = await service.GetMostSoldProductsAsync(take, null, null);
            var products = result.ToList();

            Assert.AreEqual(take, products.Count());
            Assert.AreEqual("Product1", products[0].Name);
            Assert.AreEqual(3, products[0].OrderDetails.Count);
            Assert.AreEqual("Product4", products[1].Name);
            Assert.AreEqual(2, products[1].OrderDetails.Count);
        }

        [Test]
        public async Task GetMostSoldProductsAsync_returns_products_form_selected_period_of_time()
        {
            var firstDayOfMonth = new DateTime(2018, 1, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);
            int take = 10;

            var result = await service.GetMostSoldProductsAsync(take, firstDayOfMonth, lastDayOfMonth);
            var products = result.ToList();

            Assert.AreEqual(4, products.Count());
            Assert.AreEqual("Product1", products[0].Name);
            Assert.AreEqual(2, products[0].OrderDetails.Count);

            Assert.AreEqual("Product4", products[1].Name);
            Assert.AreEqual(2, products[1].OrderDetails.Count);

            Assert.AreEqual("Product2", products[2].Name);
            Assert.AreEqual(1, products[2].OrderDetails.Count);

            Assert.AreEqual("Product5", products[3].Name);
            Assert.AreEqual(1, products[3].OrderDetails.Count);
        }

        [Test]
        public async Task GetBestCustomersAsync_returns_customers_with_highest_sum_of_orders_total_ammount()
        {
            var result = await service.GetBestCustomersAsync(10);

            Assert.AreEqual("User2", result.First().Key.FirstName);
            Assert.AreEqual(2684, result.First().Value);

            Assert.AreEqual("User1", result.Keys.ToList()[1].FirstName);
            Assert.AreEqual(1184, result.Values.ToList()[1]);

            Assert.AreEqual("User3", result.Last().Key.FirstName);
            Assert.AreEqual(126, result.Last().Value);
        }

        [Test]
        public async Task GetBestCustomersAsync_returns_selected_ammount_of_customers_with_highest_sum_of_orders_total_ammount()
        {
            int take = 2;

            var result = await service.GetBestCustomersAsync(take);

            Assert.AreEqual(take, result.Count);
            Assert.AreEqual("User2", result.First().Key.FirstName);
            Assert.AreEqual(2684, result.First().Value);
            Assert.AreEqual("User1", result.Keys.ToList()[1].FirstName);
            Assert.AreEqual(1184, result.Values.ToList()[1]);
        }

        [Test]
        public async Task GetBestOrdersAsync_returns_orders_ordered_descending_by_TotalAmmount()
        {
            var result = await service.GetBestOrdersAsync(10);
            var resultList = result.ToList();

            Assert.AreEqual(2684, resultList[0].TotalAmount);
            Assert.AreEqual(728, resultList[1].TotalAmount);
            Assert.AreEqual(456, resultList[2].TotalAmount);
            Assert.AreEqual(126, resultList[3].TotalAmount);
            Assert.That(result, Is.Ordered.Descending.By("TotalAmount"));
        }

        [Test]
        public async Task GetBestOrdersAsync_returns_selected_ammount_of_orders_ordered_descending_by_TotalAmmount()
        {
            int take = 2;
            var result = await service.GetBestOrdersAsync(take);
            var resultList = result.ToList();

            Assert.AreEqual(take, result.Count());
            Assert.AreEqual(2684, resultList[0].TotalAmount);
            Assert.AreEqual(728, resultList[1].TotalAmount);
            Assert.That(result, Is.Ordered.Descending.By("TotalAmount"));
        }

        [Test]
        public async Task GetOrdersWhichContainsSelectedProductAsync_returns_orders()
        {
            var result = await service.GetOrdersWhichContainsSelectedProductAsync(1, null, 10);

            var orders = result.Item1.ToList();

            Assert.AreEqual(1, orders[0].OrderId);
            Assert.AreEqual(2, orders[1].OrderId);
            Assert.AreEqual(4, orders[2].OrderId);
            Assert.AreEqual(3, orders.Count);
        }

        [Test]
        public async Task GetOrdersWhichContainsSelectedProductAsync_returns_paged_orders()
        {
            int pageNumber = 2;
            int pageSize = 1;

            var result = await service.GetOrdersWhichContainsSelectedProductAsync(1, pageNumber, pageSize);

            var orders = result.Item1.ToList();

            Assert.AreEqual(3, result.Item2);//orders total count for paging
            Assert.AreEqual(2, orders[0].OrderId);
            Assert.AreEqual(pageSize, orders.Count);
        }

        [Test]
        public async Task GetSalesSummaryYearsAsync_returns_years_which_contains_sales_summary()
        {
            SaleSummary styczen = new SaleSummary { SaleSummaryId = 1, MonthName = "styczeń", Year = 2018, Summary = 45698 };
            context.SalesSummary.Add(styczen);
            SaleSummary luty = new SaleSummary { SaleSummaryId = 2, MonthName = "luty", Year = 2018, Summary = 56732 };
            context.SalesSummary.Add(luty);
            SaleSummary grudzien = new SaleSummary { SaleSummaryId = 3, MonthName = "grudzień", Year = 2017, Summary = 36874 };
            context.SalesSummary.Add(grudzien);
            SaleSummary listopad = new SaleSummary { SaleSummaryId = 4, MonthName = "listopad", Year = 2017, Summary = 28469 };
            context.SalesSummary.Add(listopad);
            SaleSummary pazdziernik = new SaleSummary { SaleSummaryId = 5, MonthName = "październik", Year = 2017, Summary = 22456 };
            context.SalesSummary.Add(pazdziernik);

            var result = await service.GetSalesSummaryYearsAsync();
            var resultList = result.ToList();

            Assert.AreEqual(2, resultList.Count);
            Assert.AreEqual(2018, resultList[0]);
            Assert.AreEqual(2017, resultList[1]);
        }

        [Test]
        public async Task GetSalesSummaryForYearAsync_returns_sales_summary_for_selected_year()
        {
            SaleSummary styczen = new SaleSummary { SaleSummaryId = 1, MonthName = "styczeń", Year = 2018, Summary = 45698 };
            context.SalesSummary.Add(styczen);
            SaleSummary luty = new SaleSummary { SaleSummaryId = 2, MonthName = "luty", Year = 2018, Summary = 56732 };
            context.SalesSummary.Add(luty);
            SaleSummary grudzien = new SaleSummary { SaleSummaryId = 3, MonthName = "grudzień", Year = 2017, Summary = 36874 };
            context.SalesSummary.Add(grudzien);
            SaleSummary listopad = new SaleSummary { SaleSummaryId = 4, MonthName = "listopad", Year = 2017, Summary = 28469 };
            context.SalesSummary.Add(listopad);
            SaleSummary pazdziernik = new SaleSummary { SaleSummaryId = 5, MonthName = "październik", Year = 2017, Summary = 22456 };
            context.SalesSummary.Add(pazdziernik);

            var result = await service.GetSalesSummaryForYearAsync(2018);
            var resultList = result.ToList();

            Assert.AreEqual(2, resultList.Count);
            Assert.AreEqual(styczen, resultList[0]);
            Assert.AreEqual(luty, resultList[1]);
        }

        [Test]
        public async Task GetTopRatedProductsAsync_returns_selected_ammount_of_products()
        {
            int take = 2;
            var result = await service.GetTopRatedProductsAsync(take);

            Assert.AreEqual(take, result.Count);
        }

        [Test]
        public async Task GetTopRatedProductsAsync_returns_products_average_rate_and_rates_count_ordered_descending_by_average_rate_and_then_by_rates_count()
        {
            int take = 10;
            var result = await service.GetTopRatedProductsAsync(take);

            var resultKeys = result.Keys.ToList();

            Assert.AreEqual(product2, resultKeys[0]);
            Assert.AreEqual(product1, resultKeys[1]);
            Assert.AreEqual(product3, resultKeys[2]);

            Assert.AreEqual(result[product2], Tuple.Create(2, 4.5));
            Assert.AreEqual(result[product1], Tuple.Create(2, 4));
            Assert.AreEqual(result[product3], Tuple.Create(1, 4));
        }
    }
}