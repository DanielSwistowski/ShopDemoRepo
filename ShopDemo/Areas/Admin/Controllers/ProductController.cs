using AutoMapper;
using DataAccessLayer.Models;
using PagedList;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
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
            TempData["actionName"] = "Index";

            Dictionary<string, IEnumerable<string>> searchParams = searchDetailsParams.ToDictionary();
            var products = await productService.PageAllAsync(pageNumber, PageSize, search, priceFrom, priceTo, categoryId, true, null, false, searchParams);
            IEnumerable<AdminIndexProductViewModel> model = mapper.Map<IEnumerable<AdminIndexProductViewModel>>(products.Item1);
            int productsCount = products.Item2;
            var pagedList = new StaticPagedList<AdminIndexProductViewModel>(model, pageNumber, PageSize, productsCount);
            return View(pagedList);
        }

        public async Task<ActionResult> ProductsDeletedFromOffer([Bind(Prefix = "strona")]int? page, [Bind(Prefix = "szukaj")]string search, int? categoryId, string categoryName)
        {
            int pageNumber = (page ?? 1);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = categoryName.ToSeoUrl();
            TempData["actionName"] = "ProductsDeletedFromOffer";

            var products = await productService.PageAllAsync(pageNumber, PageSize, search, null, null, categoryId, false, null, false, null);
            IEnumerable<ProductDeletedFromOfferViewModel> model = mapper.Map<IEnumerable<ProductDeletedFromOfferViewModel>>(products.Item1);
            int productsCount = products.Item2;
            var pagedList = new StaticPagedList<ProductDeletedFromOfferViewModel>(model, pageNumber, PageSize, productsCount);
            return View(pagedList);
        }

        public async Task<ActionResult> ProductsOnPromotion([Bind(Prefix = "strona")]int? page, [Bind(Prefix = "szukaj")]string search, int? categoryId, string categoryName)
        {
            int pageNumber = (page ?? 1);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = categoryName.ToSeoUrl();
            TempData["actionName"] = "ProductsOnPromotion";

            var products = await productService.PageAllAsync(pageNumber, PageSize, search, null, null, categoryId, true, true, false, null);
            IEnumerable<ProductOnPromotionViewModel> model = mapper.Map<IEnumerable<ProductOnPromotionViewModel>>(products.Item1);
            int productsCount = products.Item2;
            var pagedList = new StaticPagedList<ProductOnPromotionViewModel>(model, pageNumber, PageSize, productsCount);
            return View(pagedList);
        }

        public async Task<ActionResult> ProductDetails(string productName, int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperties = { "ProductGallery", "ProductDetails", "ProductDiscount" };
            Product product = await productService.FindByPredicateAsync(p => p.ProductId == productId, includedProperties);
            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            ViewBag.PreviousActionName = TempData["actionName"] == null ? "Index" : TempData["actionName"];

            AdminProductDetailsViewModel model = mapper.Map<AdminProductDetailsViewModel>(product);
            return View(model);
        }

        public ActionResult AddProduct()
        {
            return View(new AddProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProduct(AddProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = mapper.Map<Product>(model);

                List<ProductCategory> productCategoryList = new List<ProductCategory>();
                foreach (var categoryId in model.SelectedCategories)
                {
                    productCategoryList.Add(new ProductCategory { CategoryId = categoryId, ProductId = product.ProductId });
                }
                product.ProductCategory = productCategoryList;

                await productService.AddAsync(product);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<ActionResult> EditProduct(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperties = { "ProductGallery", "ProductDetails", "ProductCategory" };
            Product product = await productService.FindByPredicateAsync(p => p.ProductId == productId, includedProperties);
            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            EditProductViewModel model = mapper.Map<EditProductViewModel>(product);
            model.ReturnUrl = HttpContext.Request.UrlReferrer == null ? Url.Action("Index", "Product") : HttpContext.Request.UrlReferrer.ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct(EditProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = mapper.Map<Product>(model);
                await productService.UpdateAsync(product);
                return Redirect(model.ReturnUrl);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> DeleteProduct(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = await productService.FindByIdAsync(productId);

            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            bool existstInOrders = await productService.ProductExistsInOrders(productId);
            if (existstInOrders)
                productId = 0;
            else
                await productService.DeleteAsync(product);

            return Json(productId, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> DeleteProductFromOffer(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = await productService.FindByPredicateAsync(p => p.ProductId == productId, new string[] { "ProductDiscount" });

            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (product.ProductDiscount != null)
            {
                if (product.ProductDiscount.Status != ProductDiscountStatus.Ended)
                    return Json(new { success = false, message = "Aby wycofać z oferty produkt promocyjny należy najpierw anulować promocję!" }, JsonRequestBehavior.AllowGet);
            }

            await productService.RemoveProductFromOffer(product);
            return Json(new { success = true, productId = productId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> AddProductToOffer(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = await productService.FindByIdAsync(productId);

            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            await productService.AddProductToOffer(product);
            return Json(productId, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> ActualizeProductQuantity(int quantity, int productId = 0)
        {
            if (productId == 0 || quantity < 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int updatedQuantity = await productService.ActualizeProductQuantityAsync(productId, quantity);

            return Json(new { success = true, updatedQuantity = updatedQuantity }, JsonRequestBehavior.AllowGet);
        }
    }
}