using DataAccessLayer;
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Service_Layer.Services
{
    public interface IStatisticsService
    {
        Task<IEnumerable<Product>> GetMostSoldProductsAsync(int take, DateTime? from, DateTime? to);

        Task<Dictionary<ApplicationUser, decimal>> GetBestCustomersAsync(int take);//Dictionary<User,userOrdersTotalPrice>

        Task<IEnumerable<Order>> GetBestOrdersAsync(int take);

        Task<Dictionary<Delivery, int>> GetMostPopularDeliveryOptionsAsync();//Dictionary<Delivery,deliveryCountInOrders>

        Task<Tuple<IEnumerable<Order>, int>> GetOrdersWhichContainsSelectedProductAsync(int productId, int? pageNumber, int pageSize);//Tuple<Orders, totalOrdersCountForPaging>

        Task<IEnumerable<SaleSummary>> GetSalesSummaryForYearAsync(int year);

        Task<IEnumerable<int>> GetSalesSummaryYearsAsync();

        Task<Dictionary<Product, Tuple<int, double>>> GetTopRatedProductsAsync(int take);//Dictionary<Product, Tuple<ratesCount, averageRate>>
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly IApplicationDbContext context;
        public StatisticsService(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Product>> GetMostSoldProductsAsync(int take, DateTime? from, DateTime? to)
        {
            List<Product> productsList = new List<Product>();

            IEnumerable<Product> products;
            IEnumerable<OrderDetails> orderDets;

            if (from != null && to != null)
            {
                products = await context.Orders.Where(o => o.OrderStatus == OrderStatus.Completed && o.OrderRealizationDate >= from && o.OrderRealizationDate <= to)
                .SelectMany(o => o.OrderDetails).Select(p => p.Product).Distinct().Where(o => o.OrderDetails.Count() != 0).OrderByDescending(p => p.OrderDetails.Count()).Take(take).ToListAsync();

                IEnumerable<int> productsIds = products.Select(p => p.ProductId);

                orderDets = await context.Orders.Where(o => o.OrderStatus == OrderStatus.Completed && o.OrderRealizationDate >= from && o.OrderRealizationDate <= to)
                .SelectMany(o => o.OrderDetails).Where(s => productsIds.Contains(s.ProductId)).ToListAsync();
            }
            else
            {
                products = await context.Orders.Where(o => o.OrderStatus == OrderStatus.Completed).SelectMany(o => o.OrderDetails).Select(p => p.Product).Distinct().Where(o => o.OrderDetails.Count() != 0).OrderByDescending(p => p.OrderDetails.Count()).Take(take).ToListAsync();
                IEnumerable<int> productsIds = products.Select(p => p.ProductId);
                orderDets = await context.Orders.Where(o => o.OrderStatus == OrderStatus.Completed).SelectMany(o => o.OrderDetails).Where(s => productsIds.Contains(s.ProductId)).ToListAsync();
            }

            foreach (var item in products)
            {
                IEnumerable<OrderDetails> orderDetails = orderDets.Where(p => p.ProductId == item.ProductId);

                productsList.Add(new Product { ProductId = item.ProductId, Name = item.Name, OrderDetails = orderDetails.ToList() });
            }

            return productsList.OrderByDescending(o => o.OrderDetails.Count());
        }

        public async Task<Dictionary<ApplicationUser, decimal>> GetBestCustomersAsync(int take)
        {
            Dictionary<ApplicationUser, decimal> customerOrders = await context.Orders.Include(u => u.User).Where(o => o.OrderStatus == OrderStatus.Completed)
                .GroupBy(u => u.User).Select(g => new { user = g.Key, total = g.Sum(o => o.TotalAmount) }).OrderByDescending(t => t.total).Take(take).ToDictionaryAsync(k => k.user, s => s.total);

            return customerOrders;
        }

        public async Task<IEnumerable<Order>> GetBestOrdersAsync(int take)
        {
            return await context.Orders.Include(d => d.OrderDetails).Where(o => o.OrderStatus == OrderStatus.Completed).OrderByDescending(o => o.TotalAmount).Take(take).ToListAsync();
        }

        public async Task<Dictionary<Delivery, int>> GetMostPopularDeliveryOptionsAsync()
        {
            Dictionary<Delivery, int> deliveryOptions = await context.Orders.Include(d => d.DeliveryOption).Where(o => o.OrderStatus == OrderStatus.Completed)
                .GroupBy(o => o.DeliveryOption).Select(g => new { option = g.Key, orderCount = g.Count() }).OrderByDescending(e => e.orderCount).ToDictionaryAsync(k => k.option, v => v.orderCount);

            return deliveryOptions;
        }

        public async Task<Tuple<IEnumerable<Order>, int>> GetOrdersWhichContainsSelectedProductAsync(int productId, int? pageNumber, int pageSize)
        {
            IQueryable<int> orderDetailsIdsQuery = context.OrderDetails.Where(p => p.ProductId == productId).OrderBy(o => o.OrderId).Select(i => i.OrderId);

            int count = await orderDetailsIdsQuery.CountAsync();

            if (pageNumber != null)
                orderDetailsIdsQuery = orderDetailsIdsQuery.Skip(((int)pageNumber - 1) * pageSize).Take(pageSize);

            IEnumerable<Order> orders = await context.Orders.Include(o => o.OrderDetails).Where(o => orderDetailsIdsQuery.Contains(o.OrderId)).ToListAsync();

            return Tuple.Create(orders, count);
        }

        public async Task<IEnumerable<SaleSummary>> GetSalesSummaryForYearAsync(int year)
        {
            return await context.SalesSummary.Where(r => r.Year == year).ToListAsync();
        }

        public async Task<IEnumerable<int>> GetSalesSummaryYearsAsync()
        {
            return await context.SalesSummary.Select(r => r.Year).Distinct().ToListAsync();
        }

        public async Task<Dictionary<Product, Tuple<int, double>>> GetTopRatedProductsAsync(int take)
        {
            return await context.ProductRates.GroupBy(p => p.Product).Select(g => new { product = g.Key, ratesCount = g.Count(), averageRate = g.Average(r => r.Rate) })
                .OrderByDescending(o => o.averageRate).ThenByDescending(o => o.ratesCount).Take(take).ToDictionaryAsync(d => d.product, v => Tuple.Create(v.ratesCount, v.averageRate));
        }
    }
}