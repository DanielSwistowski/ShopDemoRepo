using AutoMapper;
using DataAccessLayer.Models;
using PagedList;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    public class ProductController : AsyncController
    {
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private int PageSize = int.Parse(System.Configuration.ConfigurationManager.AppSettings["PageSize"]);

        public ProductController(IProductService productService, IMapper mapper)
        {
            this.productService = productService;
            this.mapper = mapper;
        }

        public async Task<ActionResult> Index([Bind(Prefix = "strona")]int? page, [Bind(Prefix = "szukaj")]string search, int? categoryId, string categoryName,
            [Bind(Prefix = "filtr")]string searchDetailsParams, [Bind(Prefix = "cena_od")]decimal? priceFrom, [Bind(Prefix = "cena_do")]decimal? priceTo)
        {
            int pageNumber = (page ?? 1);
            ViewBag.Search = search;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = categoryName.ToSeoUrl();
            ViewBag.SearchDetailParamsDictionary = searchDetailsParams;

            Dictionary<string, IEnumerable<string>> searchParams = searchDetailsParams.ToDictionary();
            var products = await productService.PageAllAsync(pageNumber, PageSize, search, priceFrom, priceTo, categoryId, true, null, true, searchParams);
            IEnumerable<CustomerProductViewModel> prods = mapper.Map<IEnumerable<CustomerProductViewModel>>(products.Item1);
            int productsCount = products.Item2;
            var pagedList = new StaticPagedList<CustomerProductViewModel>(prods, pageNumber, PageSize, productsCount);
            return View(pagedList);
        }

        public async Task<ActionResult> ProductDetails(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperties = { "ProductGallery", "ProductDetails", "ProductDiscount" };
            Product product = await productService.FindByPredicateAsync(p => p.ProductId == productId, includedProperties);

            if (product == null || !product.IsInOffer)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            CustomerProductDetailsViewModel model = mapper.Map<CustomerProductDetailsViewModel>(product);

            return View(model);
        }

        public async Task<ActionResult> GetProductCount(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int productCount = await productService.GetProductCountAsync(productId);

            return Json(new { success = true, productCount = productCount }, JsonRequestBehavior.AllowGet);
        }
    }
}