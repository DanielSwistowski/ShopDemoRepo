using AutoMapper;
using DataAccessLayer.Models;
using PagedList;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService statisticsService;
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private int PageSize = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TablePageSize"]);

        public StatisticsController(IStatisticsService statisticsService, IProductService productService, IMapper mapper)
        {
            this.statisticsService = statisticsService;
            this.productService = productService;
            this.mapper = mapper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> MostSoldProducts(int take = 10, string monthName = "", int year = 0)
        {
            ViewBag.SelectedCount = take;
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(monthName) && year != 0)
            {
                ViewBag.MonthName = monthName;
                ViewBag.Year = year;
                int monthNumber = DateHelper.GetMonthNumber(monthName);
                var firstDayOfMonth = new DateTime(year, monthNumber, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);
                products = await statisticsService.GetMostSoldProductsAsync(take, firstDayOfMonth, lastDayOfMonth);
            }
            else
            {
                products = await statisticsService.GetMostSoldProductsAsync(take, null, null);
            }

            IEnumerable<ProductSaleStatisticsViewModel> viewModel = mapper.Map<IEnumerable<ProductSaleStatisticsViewModel>>(products);

            return View(viewModel);
        }
        
        public async Task<ActionResult> BestCustomers(int take = 10)
        {
            ViewBag.SelectedCount = take;

            Dictionary<ApplicationUser, decimal> customers = await statisticsService.GetBestCustomersAsync(take);

            IEnumerable<BestCustomersViewModel> customersViewModel = mapper.Map<IEnumerable<BestCustomersViewModel>>(customers);

            return View(customersViewModel);
        }

        public async Task<ActionResult> BestOrders(int take = 10)
        {
            ViewBag.SelectedCount = take;

            IEnumerable<Order> orders = await statisticsService.GetBestOrdersAsync(take);

            IEnumerable<OrderStatisticsViewModel> ordersModel = mapper.Map<IEnumerable<OrderStatisticsViewModel>>(orders);

            return View(ordersModel);
        }

        public async Task<ActionResult> ProductOrderHistory(string productName, [Bind(Prefix ="strona")]int? page, int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int pageNumber = page ?? 1;

            ViewBag.ProductId = productId;
            ViewBag.ProductName = await productService.GetProductNameByProductIdAsync(productId);

            var orders = await statisticsService.GetOrdersWhichContainsSelectedProductAsync(productId, pageNumber, PageSize);

            IEnumerable<ProductOrderHistoryViewModel> productOrderHistoryViewModel = mapper.Map<IEnumerable<ProductOrderHistoryViewModel>>(orders.Item1);

            foreach (var order in productOrderHistoryViewModel)
            {
                IEnumerable<OrderDetails> orderDetails = orders.Item1.Where(o => o.OrderId == order.OrderId).SelectMany(p => p.OrderDetails);
                order.OrderedProductCount = orderDetails.Where(p => p.ProductId == productId).Sum(p => p.ProductQuantity);
            }

            var pagedList = new StaticPagedList<ProductOrderHistoryViewModel>(productOrderHistoryViewModel, pageNumber, PageSize, orders.Item2);

            return View(pagedList);
        }

        public async Task<ActionResult> BestDeliveryOptions()
        {
            Dictionary<Delivery, int> deliveryOptions = await statisticsService.GetMostPopularDeliveryOptionsAsync();

            IEnumerable<MostPopularDeliveryOptionsViewModel> deliveryOptionsViewModel = mapper.Map<IEnumerable<MostPopularDeliveryOptionsViewModel>>(deliveryOptions);

            return View(deliveryOptionsViewModel);
        }

        public async Task<ActionResult> SalesSummary()
        {
            var yearsList = await statisticsService.GetSalesSummaryYearsAsync();
            IEnumerable<SelectListItem> years = new SelectList(yearsList, yearsList.Last());
            ViewBag.Years = years;
            return View();
        }

        public async Task<ActionResult> GetSalesSummary(int year = 0)
        {
            if (year == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var summary = await statisticsService.GetSalesSummaryForYearAsync(year);

            IEnumerable<SalesSummaryViewModel> summaryViewModel = mapper.Map<IEnumerable<SalesSummaryViewModel>>(summary);

            return PartialView("_SalesSummaryPartial", summaryViewModel);
        }

        public async Task<ActionResult> TopRatedProducts(int take = 10)
        {
            ViewBag.SelectedCount = take;

            var products = await statisticsService.GetTopRatedProductsAsync(take);

            IEnumerable<TopRatedProductsViewModel> model = mapper.Map<IEnumerable<TopRatedProductsViewModel>>(products);

            return View(model);
        }
    }
}