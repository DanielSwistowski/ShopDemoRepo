using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Postal;
using Service_Layer.Services;
using ShopDemo.Areas.Admin;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ShopDemo.Tests.Areas.Admin
{
    [TestFixture]
    public class HangfireAutoCancelOrderTest
    {
        Mock<IOrderService> mockOrderService;
        Mock<IEmailService> mockEmailService;
        Mock<IDateTimeProvider> mockDateTimeProvider;

        [SetUp]
        public void SetUp()
        {
            mockOrderService = new Mock<IOrderService>();
            mockEmailService = new Mock<IEmailService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
        }

        [Test]
        public void CancelNotPaidOrders_cancels_orders_and_sent_emails_to_orders_owners()
        {
            mockDateTimeProvider.Setup(m => m.Now).Returns(new DateTime(2018, 02, 22, 11, 11, 11));

            List<Order> orders = new List<Order>();
            orders.Add(new Order { OrderId = 1, OrderStatus = OrderStatus.PaymentCancelled, OrderDate = new DateTime(2018, 02, 12, 16, 48, 25) });
            orders.Add(new Order { OrderId = 2, OrderStatus = OrderStatus.PaymentCancelled, OrderDate = new DateTime(2018, 02, 12, 05, 22, 11) });
            orders.Add(new Order { OrderId = 3, OrderStatus = OrderStatus.PaymentCancelled, OrderDate = new DateTime(2018, 02, 12, 09, 56, 17) });

            mockOrderService.Setup(m => m.GetAll(It.IsAny<Expression<Func<Order, bool>>>())).Returns(orders);
            mockOrderService.Setup(m => m.CancelNotPaidOrder(It.IsAny<int>())).Returns("testeemail@wp.pl");

            HangfireAutoCancelOrder service = new HangfireAutoCancelOrder(mockOrderService.Object, mockEmailService.Object, mockDateTimeProvider.Object);

            service.CancelNotPaidOrders();

            mockOrderService.Verify(m => m.CancelNotPaidOrder(It.IsAny<int>()), Times.Exactly(3));
            mockEmailService.Verify(m => m.Send(It.IsAny<AutoCancelOrderEmail>()), Times.Exactly(3));
        }
    }
}