using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService cartService;
        private readonly IProductService productService;
        private readonly IMapper mapper;

        public CartController(ICartService cartService, IProductService productService, IMapper mapper)
        {
            this.cartService = cartService;
            this.productService = productService;
            this.mapper = mapper;
        }

        public PartialViewResult ShoppingCartLink()
        {
            int cartTotalItemsCount = cartService.GetCart().TotalItemsCount;
            return PartialView("_CartLinkPartial", cartTotalItemsCount);
        }

        public ActionResult GetCart()
        {
            ShoppingCart cart = cartService.GetCart();

            string url = Url.Action("Index", "Product");
            if(TempData["urlReferrer"] != null)
                url = TempData["urlReferrer"] as string;
            else
            {
                var referrerUrl = HttpContext.Request.UrlReferrer;
                if (referrerUrl != null)
                {
                    var cartUrl = Url.Action("GetCart", "Cart");
                    if (referrerUrl.AbsolutePath != cartUrl)
                    {
                        url = referrerUrl.ToString();
                    }
                }
            }
            ViewBag.ReferrerUrl = url;

            ShoppingCartViewModel model = mapper.Map<ShoppingCartViewModel>(cart);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> AddToCart(int productId, int productCount)
        {
            Product product = await productService.FindByPredicateAsync(p => p.ProductId == productId, new string[] { "ProductDiscount" });
            CartItem item = new CartItem();
            item.ProductId = productId;
            item.Product = product;
            item.ProductCount = productCount;

            if (product.ProductDiscount != null)
            {
                if(product.ProductDiscount.Status == ProductDiscountStatus.DuringTime)
                    item.ProductPrice = GetProductPromotionPrice(product.Price, product.ProductDiscount.DiscountQuantity);
                else
                    item.ProductPrice = product.Price;
            }
            else
            {
                item.ProductPrice = product.Price;
            }

            cartService.AddToCart(item);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public decimal GetProductPromotionPrice(decimal productPrice, int productDiscount)
        {
            return (productPrice - (productPrice * productDiscount / 100));
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public JsonResult EmptyCart()
        {
            cartService.EmptyCart();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public ActionResult RemoveProductFromCart(string urlReferrer, int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TempData["urlReferrer"] = urlReferrer;

            cartService.RemoveFromCart(productId);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public ActionResult UpdateProductCount(string urlReferrer, int productCount, int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TempData["urlReferrer"] = urlReferrer;

            cartService.UpdateCartProductCount(productId, productCount);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductCount(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int productCount = cartService.GetProductCount(productId);

            return Json(new { success = true, productCount = productCount }, JsonRequestBehavior.AllowGet);
        }
    }
}