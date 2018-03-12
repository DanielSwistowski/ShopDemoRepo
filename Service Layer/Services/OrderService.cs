using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IOrderPaymentService
    {
        Task SavePaymentAsync(int orderId, string payuStatus, string payuOrderId);
        Task<string> GetPayuOrderIdAsync(int orderId);
    }

    public interface IOrderService : IBaseService<Order>, IOrderPaymentService
    {
        Task<Dictionary<int, string>> CreateNewOrderAsync(Order order);
        Task CancelOrderAsync(int orderId, OrderStatus status);
        string CancelNotPaidOrder(int orderId);
        Task ChangeStatusAsync(int orderId, OrderStatus status);
        void ChangeStatusToUncompleted(int orderId);
        Task<Dictionary<int, string>> UpdateOrderAsync(int orderId, IEnumerable<OrderDetails> orderDetails);
        Task<string> FindUserEmailByOrderId(int orderId);
        Task RemoveOrderFormCustomerOrdersListAsync(int orderId);
        Task<int> CreateNewOrderIdAsync(Order existingOrder);
    }

    public class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IApplicationDbContext context;
        public OrderService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task CancelOrderAsync(int orderId, OrderStatus status)
        {
            Order order = await context.Orders.FindAsync(orderId);
            order.OrderStatus = status;
            context.SetModified(order);

            var products = order.OrderDetails;

            foreach (var product in products)
            {
                Product productToUpdate = await context.Products.FindAsync(product.ProductId);
                if (productToUpdate != null)
                {
                    productToUpdate.Quantity = productToUpdate.Quantity + product.ProductQuantity;
                    context.SetModified(productToUpdate);
                }
            }

            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException dx)
                {
                    saveFailed = true;
                    var entries = dx.Entries;
                    foreach (var entry in entries)
                    {
                        var clientValues = (Product)entry.Entity;
                        var databaseEntry = await entry.GetDatabaseValuesAsync();
                        if (databaseEntry != null)
                        {
                            var databaseValues = (Product)databaseEntry.ToObject();
                            int productQuantity = order.OrderDetails.Where(x => x.ProductId == clientValues.ProductId).Select(p => p.ProductQuantity).Single();
                            databaseValues.Quantity = databaseValues.Quantity + productQuantity;
                            entry.OriginalValues.SetValues(databaseEntry);
                            entry.CurrentValues.SetValues(databaseValues);
                        }
                    }
                }
            } while (saveFailed);
        }

        public string CancelNotPaidOrder(int orderId)
        {
            Order order = context.Orders.Include(u=>u.User).Where(o=>o.OrderId == orderId).Single();
            string email = order.User.Email;

            if (order.OrderStatus != OrderStatus.PaymentCancelled)
                return "";

            order.OrderStatus = OrderStatus.CancelledByAdmin;
            context.SetModified(order);

            var products = order.OrderDetails;

            foreach (var product in products)
            {
                Product productToUpdate = context.Products.Find(product.ProductId);
                if (productToUpdate != null)
                {
                    productToUpdate.Quantity = productToUpdate.Quantity + product.ProductQuantity;
                    context.SetModified(productToUpdate);
                }
            }

            bool saveFailed;
            do
            {
                int counter = 0;
                saveFailed = false;
                try
                {
                    counter++;
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException dx)
                {
                    if (counter == 10)
                        throw new Exception("Unable to cancel order");

                    saveFailed = true;
                    var entries = dx.Entries;
                    foreach (var entry in entries)
                    {
                        var clientValues = (Product)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry != null)
                        {
                            var databaseValues = (Product)databaseEntry.ToObject();
                            int productQuantity = order.OrderDetails.Where(x => x.ProductId == clientValues.ProductId).Select(p => p.ProductQuantity).Single();
                            databaseValues.Quantity = databaseValues.Quantity + productQuantity;
                            entry.OriginalValues.SetValues(databaseEntry);
                            entry.CurrentValues.SetValues(databaseValues);
                        }
                    }
                }
            } while (saveFailed);
            return email;
        }

        public void ChangeStatusToUncompleted(int orderId)
        {
            Order order = context.Orders.Find(orderId);

            if (order.OrderStatus == OrderStatus.DuringRealization)
            {
                order.OrderStatus = OrderStatus.Uncompleted;
                context.SetModified(order);
                context.SaveChanges();
            }
        }

        public async Task ChangeStatusAsync(int orderId, OrderStatus status)
        {
            Order order = await context.Orders.FindAsync(orderId);
            order.OrderStatus = status;

            if (status == OrderStatus.Completed)
                order.OrderRealizationDate = DateTime.Now;

            context.SetModified(order);
            await context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, string>> CreateNewOrderAsync(Order order)
        {
            Dictionary<int, string> unavailableToSave = new Dictionary<int, string>();
            context.Orders.Add(order);

            foreach (var product in order.OrderDetails)
            {
                Product productToUpdate = await context.Products.FindAsync(product.ProductId);
                if (productToUpdate == null)
                {
                    unavailableToSave.Add(product.ProductId, "Produkt został usunięty ze sklepu");
                }
                else
                {
                    int updatedProductQuantity = productToUpdate.Quantity - product.ProductQuantity;
                    if (updatedProductQuantity < 0)
                    {
                        unavailableToSave.Add(product.ProductId, "Brak wymaganej ilości produktu w sklepie (dostępne tylko " + productToUpdate.Quantity + "szt.)");
                    }

                    if (!productToUpdate.IsInOffer)
                    {
                        unavailableToSave.Add(product.ProductId, "Produkt został wycofany ze sprzedaży");
                    }

                    productToUpdate.Quantity = updatedProductQuantity;
                    context.SetModified(productToUpdate);
                }
            }

            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (unavailableToSave.Count == 0)
                        await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException dx)
                {
                    saveFailed = true;
                    var entries = dx.Entries;
                    foreach (var entry in entries)
                    {
                        var clientValues = (Product)entry.Entity;
                        var databaseEntry = await entry.GetDatabaseValuesAsync();
                        if (databaseEntry == null)
                        {
                            unavailableToSave.Add(clientValues.ProductId, "Produkt został usunięty ze sklepu");
                        }
                        else
                        {
                            var databaseValues = (Product)databaseEntry.ToObject();
                            int orderedProductQuantity = order.OrderDetails.Where(x => x.ProductId == clientValues.ProductId).Select(p => p.ProductQuantity).Single();
                            int restProductQuantity = databaseValues.Quantity - orderedProductQuantity;
                            if (restProductQuantity < 0)
                            {
                                unavailableToSave.Add(clientValues.ProductId, "Brak wymaganej ilości produktu w sklepie (dostępne tylko " + databaseValues.Quantity + "szt.)");
                            }
                            else if (!databaseValues.IsInOffer)
                            {
                                unavailableToSave.Add(clientValues.ProductId, "Produkt został wycofany ze sprzedaży");
                            }
                            else
                            {
                                databaseValues.Quantity = databaseValues.Quantity - orderedProductQuantity;
                                entry.OriginalValues.SetValues(databaseEntry);
                                entry.CurrentValues.SetValues(databaseValues);
                            }
                        }
                    }
                }
            } while (saveFailed);

            return unavailableToSave;
        }

        public async Task<string> FindUserEmailByOrderId(int orderId)
        {
            var userOrder = await context.Orders.Include(u => u.User).Where(o => o.OrderId == orderId).SingleAsync();
            return userOrder.User.Email;
        }

        public async Task<Dictionary<int, string>> UpdateOrderAsync(int orderId, IEnumerable<OrderDetails> orderDetails)
        {
            Dictionary<int, string> unavailableToSave = new Dictionary<int, string>();

            IEnumerable<OrderDetails> existingOrderDetails = await context.OrderDetails.Where(o => o.OrderId == orderId).ToListAsync();

            foreach (var product in orderDetails)
            {
                Product productToUpdate = await context.Products.FindAsync(product.ProductId);
                if (productToUpdate == null)
                {
                    unavailableToSave.Add(product.ProductId, "Produkt został usunięty ze sklepu");
                }
                else
                {
                    int updatedProductQuantity = productToUpdate.Quantity - product.ProductQuantity;
                    if (updatedProductQuantity < 0)
                    {
                        unavailableToSave.Add(product.ProductId, "Brak wymaganej ilości produktu w sklepie (dostępne tylko " + productToUpdate.Quantity + "szt.)");
                    }

                    if (!productToUpdate.IsInOffer)
                    {
                        unavailableToSave.Add(product.ProductId, "Produkt został wycofany ze sprzedaży");
                    }

                    productToUpdate.Quantity = updatedProductQuantity;
                    context.SetModified(productToUpdate);
                }

                bool productExistInOrderDetails = existingOrderDetails.Where(p => p.ProductId == product.ProductId).Any();
                if (productExistInOrderDetails)
                {
                    IEnumerable<OrderDetails> productsInOrderDetail = existingOrderDetails.Where(p => p.ProductId == product.ProductId);
                    OrderDetails oD = null;
                    foreach (var item in productsInOrderDetail)
                    {
                        if (item.ProductUnitPrice == product.ProductUnitPrice) //cena sie zmieniła ponieważ np. skończył się czas promocji
                        {
                            oD = item;
                            break;
                        }
                    }
                    if (oD == null)
                    {
                        OrderDetails newOrderDetail = new OrderDetails();
                        newOrderDetail.OrderId = orderId;
                        newOrderDetail.ProductId = product.ProductId;
                        newOrderDetail.ProductQuantity = product.ProductQuantity;
                        newOrderDetail.ProductUnitPrice = product.ProductUnitPrice;
                        newOrderDetail.Total = product.Total;
                        context.OrderDetails.Add(newOrderDetail);
                    }
                    else
                    {
                        oD.ProductQuantity = oD.ProductQuantity + product.ProductQuantity;
                        oD.Total = oD.Total + product.ProductQuantity * product.ProductUnitPrice;
                        context.SetModified(oD);
                    }
                }
                else
                {
                    OrderDetails newOrderDetail = new OrderDetails();
                    newOrderDetail.OrderId = orderId;
                    newOrderDetail.ProductId = product.ProductId;
                    newOrderDetail.ProductQuantity = product.ProductQuantity;
                    newOrderDetail.ProductUnitPrice = product.ProductUnitPrice;
                    newOrderDetail.Total = product.Total;
                    context.OrderDetails.Add(newOrderDetail);
                }
            }

            Order order = await context.Orders.FindAsync(orderId);
            order.OrderDate = DateTime.Now;
            order.TotalAmount = order.TotalAmount + orderDetails.Sum(t => t.Total);
            context.SetModified(order);

            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (unavailableToSave.Count == 0)
                        await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException dx)
                {
                    saveFailed = true;
                    var entries = dx.Entries;
                    foreach (var entry in entries)
                    {
                        var type = entry.Entity.GetType();

                        if (type.BaseType == typeof(Order)) // order exception
                        {
                            var clientValues = (Order)entry.Entity;
                            var databaseEntry = await entry.GetDatabaseValuesAsync();
                            var databaseValues = (Order)databaseEntry.ToObject();
                            if (databaseValues.OrderStatus != clientValues.OrderStatus)
                            {
                                unavailableToSave.Add(0, "Nie można zaktualizować zamówienia, ponieważ jego status uległ zmianie.");
                            }
                        }

                        if (type.BaseType == typeof(Product)) //product exception
                        {
                            var clientValues = (Product)entry.Entity;
                            var databaseEntry = await entry.GetDatabaseValuesAsync();
                            if (databaseEntry == null)
                            {
                                unavailableToSave.Add(clientValues.ProductId, "Produkt został usunięty ze sklepu");
                            }
                            else
                            {
                                var databaseValues = (Product)databaseEntry.ToObject();
                                int orderedProductQuantity = orderDetails.Where(x => x.ProductId == clientValues.ProductId).Select(p => p.ProductQuantity).Single();
                                int restProductQuantity = databaseValues.Quantity - orderedProductQuantity;
                                if (restProductQuantity < 0)
                                {
                                    unavailableToSave.Add(clientValues.ProductId, "Brak wymaganej ilości produktu w sklepie (dostępne tylko " + databaseValues.Quantity + "szt.)");
                                }
                                else if (!databaseValues.IsInOffer)
                                {
                                    unavailableToSave.Add(clientValues.ProductId, "Produkt został wycofany ze sprzedaży");
                                }
                                else
                                {
                                    databaseValues.Quantity = databaseValues.Quantity - orderedProductQuantity;
                                    entry.OriginalValues.SetValues(databaseEntry);
                                    entry.CurrentValues.SetValues(databaseValues);
                                }
                            }
                        }
                    }
                }
            } while (saveFailed);

            return unavailableToSave;
        }

        public async Task SavePaymentAsync(int orderId, string payuStatus, string payuOrderId)
        {
            Order order = await context.Orders.FindAsync(orderId);
            if(order != null)
            {
                PayuOrderData existingPayuData = await context.PayuData.FindAsync(orderId);
                if (existingPayuData != null)
                {
                    existingPayuData.PaymentStatus = payuStatus;
                    existingPayuData.PayuOrderId = payuOrderId;
                    context.SetModified(existingPayuData);
                }
                else
                {
                    PayuOrderData payuData = new PayuOrderData();
                    payuData.PaymentStatus = payuStatus;
                    payuData.PayuOrderId = payuOrderId;
                    order.PayuData = payuData;
                }

                if (payuStatus == "COMPLETED")
                    order.OrderStatus = OrderStatus.Uncompleted;

                if (payuStatus == "CANCELED")
                    order.OrderStatus = OrderStatus.PaymentCancelled;

                if (payuStatus == "REJECTED")
                    order.OrderStatus = OrderStatus.PaymentRejected;

                context.SetModified(order);
                await context.SaveChangesAsync();
            }
        }

        public async Task<string> GetPayuOrderIdAsync(int orderId)
        {
            var payuData = await context.PayuData.FindAsync(orderId);
            return payuData.PayuOrderId;
        }

        public async Task RemoveOrderFormCustomerOrdersListAsync(int orderId)
        {
            Order order = await context.Orders.FindAsync(orderId);
            order.Removed = true;
            context.SetModified(order);
            await context.SaveChangesAsync();
        }

        public async Task<int> CreateNewOrderIdAsync(Order existingOrder)
        {
            Order newOrder = new Order();
            newOrder.UserId = existingOrder.UserId;
            newOrder.TotalAmount = existingOrder.TotalAmount;
            newOrder.Removed = existingOrder.Removed;
            newOrder.OrderStatus = existingOrder.OrderStatus;
            newOrder.OrderDate = existingOrder.OrderDate;
            newOrder.OrderRealizationDate = existingOrder.OrderRealizationDate;
            newOrder.DeliveryId = existingOrder.DeliveryId;
            context.Orders.Add(newOrder);

            IEnumerable<OrderDetails> existingOrderDetails = await context.OrderDetails.Where(o => o.OrderId == existingOrder.OrderId).ToListAsync();
            foreach (var orderDetail in existingOrderDetails)
            {
                orderDetail.OrderId = newOrder.OrderId;
                context.SetModified(orderDetail);
            }

            context.Orders.Remove(existingOrder);
            await context.SaveChangesAsync();
            return newOrder.OrderId;
        }
    }
}