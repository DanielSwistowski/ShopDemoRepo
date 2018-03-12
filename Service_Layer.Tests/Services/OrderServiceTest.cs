using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class OrderServiceTest
    {
        OrderService service;
        FakeDbContext context;
        Order order;

        [SetUp]
        public void SetUp()
        {
            context = new FakeDbContext();

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            orderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            context.Products.Add(new Product { ProductId = 1, Name = "Product1", Description = "Description1", Price = 10, IsInOffer = true, Quantity = 2, DeletedFromOfferDate = null, CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 2, Name = "Product2", Description = "Description2", Price = 5, IsInOffer = false, Quantity = 4, DeletedFromOfferDate = new DateTime(2017, 12, 20, 15, 15, 15), CreatedAt = DateTime.Now });
            context.Products.Add(new Product { ProductId = 3, Name = "Product3", Description = "Description3", Price = 5, IsInOffer = true, Quantity = 4, DeletedFromOfferDate = null, CreatedAt = DateTime.Now });

            order = new Order { OrderId = 1, OrderDate = DateTime.Now, OrderRealizationDate = null, OrderStatus = OrderStatus.Uncompleted, TotalAmount = 45, OrderDetails = orderDetails, Removed = false, User = new ApplicationUser { Email = "jankowalski@gmail.com" } };
            context.Orders.Add(order);

            service = new OrderService(context);
        }

        [Test]
        public async Task CancelOrderAsync_returns_ordered_products_quantity()
        {
            int orderId = 1;
            await service.CancelOrderAsync(orderId, OrderStatus.CancelledByCustomer);

            var result = await context.Products.ToListAsync();

            Assert.IsTrue(result[0].Quantity == 4);
            Assert.IsTrue(result[1].Quantity == 9);
        }

        [Test]
        public void CancelNotPaidOrder_returns_empty_string_if_order_status_is_not_equal_to_PaymentCancelled()
        {
            int orderId = 1;
            var result =service.CancelNotPaidOrder(orderId);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [Test]
        public void CancelNotPaidOrder_returns_empty_string_if_order_status_is_not_equal_to_WaitingForPaymentComplete()
        {
            int orderId = 1;
            var result = service.CancelNotPaidOrder(orderId);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [Test]
        public void CancelNotPaidOrder_returns_order_owner_email_address()
        {
            //change status for order from setup
            order.OrderStatus = OrderStatus.PaymentCancelled;

            int orderId = 1;
            var result = service.CancelNotPaidOrder(orderId);

            Assert.AreEqual("jankowalski@gmail.com", result);
        }

        [Test]
        public void CancelNotPaidOrder_returns_ordered_products_quantity()
        {
            //change status for order from setup
            order.OrderStatus = OrderStatus.PaymentCancelled;

            int orderId = 1;
            var result = service.CancelNotPaidOrder(orderId);

            var products = context.Products.ToList();

            Assert.IsTrue(products[0].Quantity == 4);
            Assert.IsTrue(products[1].Quantity == 9);
        }

        [Test]
        public async Task ChangeStatusAsync_changes_order_status()
        {
            int orderId = 1;
            await service.ChangeStatusAsync(orderId, OrderStatus.WaitingForPaymentComplete);

            var result = await service.FindByIdAsync(orderId);

            Assert.IsTrue(result.OrderStatus == OrderStatus.WaitingForPaymentComplete);
        }

        [Test]
        public async Task ChangeStatusAsync_changes_order_status_and_set_OrderRealizationDate_if_new_order_status_is_Completed()
        {
            int orderId = 1;
            await service.ChangeStatusAsync(orderId, OrderStatus.Completed);

            var result = await service.FindByIdAsync(orderId);

            Assert.IsTrue(result.OrderStatus == OrderStatus.Completed);
            Assert.IsNotNull(result.OrderRealizationDate);
        }

        [Test]
        public async Task CreateNewOrderAsync_returns_error_if_selected_product_is_null()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            Product product = null;
            int productId = 20;
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });

            Order order = new Order { OrderDetails = orderDetails };

            Dictionary<int, string> result = await service.CreateNewOrderAsync(order);

            Assert.AreEqual("Produkt został usunięty ze sklepu", result[productId]);
        }

        [Test]
        public async Task CreateNewOrderAsync_returns_error_if_selected_product_quantity_is_greather_then_existing_product_quantity()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 1;
            Product product = new Product { ProductId = productId, Quantity = 3 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 3, ProductUnitPrice = 10, Total = 20 });

            Order order = new Order { OrderDetails = orderDetails };

            Dictionary<int, string> result = await service.CreateNewOrderAsync(order);

            Assert.AreEqual("Brak wymaganej ilości produktu w sklepie (dostępne tylko 2szt.)", result[productId]);
        }

        [Test]
        public async Task CreateNewOrderAsync_returns_error_if_selected_product_not_exists_in_offer()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 2;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 10, Total = 20 });

            Order order = new Order { OrderDetails = orderDetails };

            Dictionary<int, string> result = await service.CreateNewOrderAsync(order);

            Assert.AreEqual("Produkt został wycofany ze sprzedaży", result[productId]);
        }

        [Test]
        public async Task CreateNewOrderAsync_creates_new_order()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 1;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 10, Total = 20 });

            Order order = new Order { OrderDetails = orderDetails };

            Dictionary<int, string> result = await service.CreateNewOrderAsync(order);

            Assert.AreEqual(0, result.Count);
            Assert.IsTrue((await context.Orders.ToListAsync()).Contains(order));
        }

        [Test]
        public async Task UpdateOrderAsync_returns_error_if_selected_product_is_null()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            Product product = null;
            int productId = 20;
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            Assert.AreEqual("Produkt został usunięty ze sklepu", result[productId]);
        }

        [Test]
        public async Task UpdateOrderAsync_returns_error_if_selected_product_quantity_is_greather_then_existing_product_quantity()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 1;
            Product product = new Product { ProductId = productId, Quantity = 3 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 3, ProductUnitPrice = 10, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            Assert.AreEqual("Brak wymaganej ilości produktu w sklepie (dostępne tylko 2szt.)", result[productId]);
        }

        [Test]
        public async Task UpdateOrderAsync_returns_error_if_selected_product_not_exists_in_offer()
        {
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 2;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 10, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            Assert.AreEqual("Produkt został wycofany ze sprzedaży", result[productId]);
        }

        [Test]
        public async Task UpdateOrderAsync_increase_ordered_product_count_if_product_exists_in_existing_order_details_and_its_price_is_this_same()
        {
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 1;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 10, Total = 10 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            var updatedOrderDetails = await context.OrderDetails.ToArrayAsync();
            var updatedOrder = context.Orders.Find(1);

            Assert.IsTrue(result.Count == 0);
            Assert.AreEqual(3, updatedOrderDetails[0].ProductQuantity);
            Assert.AreEqual(30, updatedOrderDetails[0].Total);
            Assert.AreEqual(2, updatedOrderDetails.Length);
            Assert.AreEqual(55, updatedOrder.TotalAmount);
        }

        [Test]
        public async Task UpdateOrderAsync_adds_new_product_to_order_details_if_product_exists_in_existing_order_details_but_its_price_is_different()
        {
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 1;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 20, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            var updatedOrderDetails = await context.OrderDetails.ToArrayAsync();
            var updatedOrder = context.Orders.Find(1);

            Assert.IsTrue(result.Count == 0);
            Assert.AreEqual(1, updatedOrderDetails[2].ProductQuantity);
            Assert.AreEqual(3, updatedOrderDetails.Length);
            Assert.AreEqual(65, updatedOrder.TotalAmount);
        }

        [Test]
        public async Task UpdateOrderAsync_adds_new_product_to_order_details_if_product_not_exists_in_existing_order_details()
        {
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 3;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 20, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            var updatedOrderDetails = await context.OrderDetails.ToArrayAsync();
            var updatedOrder = context.Orders.Find(1);

            Assert.IsTrue(result.Count == 0);
            Assert.AreEqual(1, updatedOrderDetails[2].ProductQuantity);
            Assert.AreEqual(3, updatedOrderDetails.Length);
            Assert.AreEqual(65, updatedOrder.TotalAmount);
        }

        [Test]
        public async Task UpdateOrderAsync_decreases_ordered_product_quantity()
        {
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            List<OrderDetails> orderDetails = new List<OrderDetails>();
            int productId = 3;
            Product product = new Product { ProductId = productId, Quantity = 1 };
            orderDetails.Add(new OrderDetails { OrderDetailsId = 1, ProductId = productId, Product = product, ProductQuantity = 1, ProductUnitPrice = 20, Total = 20 });

            Dictionary<int, string> result = await service.UpdateOrderAsync(1, orderDetails);

            var orderedProduct = context.Products.Find(productId);

            Assert.IsTrue(result.Count == 0);
            Assert.AreEqual(3, orderedProduct.Quantity);
        }

        [Test]
        public async Task RemoveOrderFormCustomerOrdersListAsync_sets_order_Removed_property_to_true()
        {
            int orderId = 1;
            await service.RemoveOrderFormCustomerOrdersListAsync(orderId);

            var order = context.Orders.Find(orderId);

            Assert.IsTrue(order.Removed);
        }

        [Test]
        public async Task SavePaymentAsync_updates_PayuOrderData_if_it_exists()
        {
            PayuOrderData payuData = new PayuOrderData { OrderId = 1, PaymentStatus = "CANCELED", PayuOrderId = "GHDKJ778" };
            context.PayuData.Add(payuData);

            await service.SavePaymentAsync(1, "PaymentStatus", "PayuID");

            var payuOrderData = context.PayuData.Find(1);
            Assert.AreEqual("PaymentStatus", payuOrderData.PaymentStatus);
            Assert.AreEqual("PayuID", payuOrderData.PayuOrderId);
        }

        [Test]
        public async Task SavePaymentAsync_adds_new_PayuOrderData_if_it_not_exists()
        {
            await service.SavePaymentAsync(1, "PaymentStatus", "PayuID");

            var order = context.Orders.Find(1);
            Assert.AreEqual("PaymentStatus", order.PayuData.PaymentStatus);
            Assert.AreEqual("PayuID", order.PayuData.PayuOrderId);
        }

        [Test]
        public async Task SavePaymentAsync_sets_order_status_to_Uncompleted_if_payuStatus_is_COMPLETED()
        {
            await service.SavePaymentAsync(1, "COMPLETED", "PayuID");

            var order = context.Orders.Find(1);
            Assert.AreEqual("COMPLETED", order.PayuData.PaymentStatus);
            Assert.AreEqual(OrderStatus.Uncompleted, order.OrderStatus);
        }

        [Test]
        public async Task SavePaymentAsync_sets_order_status_to_PaymentCancelled_if_payuStatus_is_CANCELED()
        {
            await service.SavePaymentAsync(1, "CANCELED", "PayuID");

            var order = context.Orders.Find(1);
            Assert.AreEqual("CANCELED", order.PayuData.PaymentStatus);
            Assert.AreEqual(OrderStatus.PaymentCancelled, order.OrderStatus);
        }

        [Test]
        public async Task SavePaymentAsync_sets_order_status_to_PaymentRejected_if_payuStatus_is_REJECTED()
        {
            await service.SavePaymentAsync(1, "REJECTED", "PayuID");

            var order = context.Orders.Find(1);
            Assert.AreEqual("REJECTED", order.PayuData.PaymentStatus);
            Assert.AreEqual(OrderStatus.PaymentRejected, order.OrderStatus);
        }

        [Test]
        public async Task GetPayuOrderIdAsync_returns_PayuOrderId()
        {
            PayuOrderData payuData = new PayuOrderData { OrderId = 1, PaymentStatus = "CANCELED", PayuOrderId = "GHDKJ778" };
            context.PayuData.Add(payuData);

            string result = await service.GetPayuOrderIdAsync(1);

            Assert.AreEqual("GHDKJ778", result);
        }

        [Test]
        public void ChangeStatusToUncompleted_changes_status_if_actual_order_status_is_DuringRealization()
        {
            Order order = new Order { OrderId = 10, OrderStatus = OrderStatus.DuringRealization };
            context.Orders.Add(order);

            service.ChangeStatusToUncompleted(10);

            var updatedOrder = context.Orders.Find(10);

            Assert.AreEqual(OrderStatus.Uncompleted, updatedOrder.OrderStatus);
        }

        [Test]
        public async Task FindUserEmailByOrderId_returns_user_email()
        {
            Order order = new Order { OrderId=10, User = new ApplicationUser { Email = "testemail@email.com" } };
            context.Orders.Add(order);

            string email = await service.FindUserEmailByOrderId(10);

            Assert.AreEqual("testemail@email.com", email);
        }

        [Test]
        public async Task CreateNewOrderIdAsync_creates_new_order_with_data_from_existing_order_and_returns_new_OrderId()
        {
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 1, OrderId = 1, ProductId = 1, ProductQuantity = 2, ProductUnitPrice = 10, Total = 20 });
            context.OrderDetails.Add(new OrderDetails { OrderDetailsId = 2, OrderId = 1, ProductId = 2, ProductQuantity = 5, ProductUnitPrice = 5, Total = 25 });

            int newOrderId = await service.CreateNewOrderIdAsync(order);

            var newOrder = context.Orders.Find(newOrderId);
            var updatedOrderDetails = context.OrderDetails.Where(o => o.OrderId == newOrderId).ToList();
            var oldOrderDetails = context.OrderDetails.Where(o => o.OrderId == 1).ToList();

            Assert.AreEqual(0, newOrderId);
            Assert.AreEqual(0, oldOrderDetails.Count);
            Assert.AreEqual(2, updatedOrderDetails.Count);
        }
    }
}