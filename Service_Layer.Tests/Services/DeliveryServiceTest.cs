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
    public class DeliveryServiceTest
    {
        DeliveryService service;
        FakeDbContext context;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            context.Orders.Add(new Order { OrderId = 1, DeliveryId = 1 });
            context.Orders.Add(new Order { OrderId = 2, DeliveryId = 2 });
            context.Orders.Add(new Order { OrderId = 3, DeliveryId = 3 });


            context.Deliveries.Add(new Delivery { DeliveryId = 1, IsActive = true });
            context.Deliveries.Add(new Delivery { DeliveryId = 2, IsActive = false });
            context.Deliveries.Add(new Delivery { DeliveryId = 3, IsActive = true });

            service = new DeliveryService(context);
        }

        [Test]
        public async Task DeliveryExistsInOrdersAsync_returns_true_if_selected_delivery_exists_in_orders()
        {
            int deliveryId = 1;

            var result = await service.DeliveryExistsInOrdersAsync(deliveryId);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeliveryExistsInOrdersAsync_returns_false_if_selected_delivery_not_exists_in_orders()
        {
            int deliveryId = 12;

            var result = await service.DeliveryExistsInOrdersAsync(deliveryId);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeactivateDeliveryOptionAsync_sets_delivery_IsActive_property_to_false()
        {
            int deliveryId = 1;

            await service.DeactivateDeliveryOptionAsync(deliveryId);

            var delivery = context.Deliveries.Find(deliveryId);

            Assert.IsFalse(delivery.IsActive);
        }

        [Test]
        public async Task ActivateDeliveryOptionAsync_sets_delivery_IsActive_property_to_true()
        {
            int deliveryId = 2;

            await service.ActivateDeliveryOptionAsync(deliveryId);

            var delivery = context.Deliveries.Find(deliveryId);

            Assert.IsTrue(delivery.IsActive);
        }
    }
}
