using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Linq;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class SalesSummaryServiceTest
    {
        Mock<IDateTimeProvider> mockTimeProvider;
        FakeDbContext context;
        SalesSummaryService service;

        [SetUp]
        public void SetUp()
        {
            mockTimeProvider = new Mock<IDateTimeProvider>();
            mockTimeProvider.Setup(m => m.Now).Returns(new DateTime(2018, 02, 16, 14, 28, 45));

            context = new FakeDbContext();
            context.Orders.Add(new Order { OrderId = 1, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), TotalAmount = 456 });
            context.Orders.Add(new Order { OrderId = 2, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 02, 22, 15, 22, 08), TotalAmount = 2684 });
            context.Orders.Add(new Order { OrderId = 3, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), TotalAmount = 126 });
            context.Orders.Add(new Order { OrderId = 4, OrderStatus = OrderStatus.Completed, OrderRealizationDate = new DateTime(2018, 01, 22, 15, 22, 08), TotalAmount = 728 });

            service = new SalesSummaryService(context, mockTimeProvider.Object);
        }

        [Test]
        public void CalculateSalesSummaryForPreviousMonth_adds_sales_summary_if_it_not_exists()
        {
            service.CalculateSalesSummaryForPreviousMonth();

            var summary = context.SalesSummary.ToList();

            Assert.AreEqual(1, summary.Count);
            Assert.AreEqual(1310, summary.First().Summary);
        }

        [Test]
        public void CalculateSalesSummaryForPreviousMonth_not_saves_sales_summary_if_it_exists()
        {
            context.SalesSummary.Add(new SaleSummary { SaleSummaryId = 1, MonthName = "styczeń", Year = 2018, Summary = 45356 });

            service.CalculateSalesSummaryForPreviousMonth();

            var summary = context.SalesSummary.ToList();

            Assert.AreEqual(1, summary.Count);
            Assert.AreEqual(45356, summary.First().Summary);
        }
    }
}
