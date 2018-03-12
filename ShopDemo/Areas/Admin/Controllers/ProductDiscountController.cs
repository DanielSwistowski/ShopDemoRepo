using AutoMapper;
using DataAccessLayer.Models;
using Hangfire;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductDiscountController : Controller
    {
        private readonly IProductService productService;
        private readonly IProductDiscountService productDiscountService;
        private readonly IMapper mapper;
        private readonly IBackgroundJobClient jobClient;

        public ProductDiscountController(IProductService productService, IProductDiscountService productDiscountService, IMapper mapper, IBackgroundJobClient jobClient)
        {
            this.productService = productService;
            this.productDiscountService = productDiscountService;
            this.mapper = mapper;
            this.jobClient = jobClient;
        }

        public async Task<ActionResult> Details(int productId= 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperty = { "Product" };
            ProductDiscount productDiscount = await productDiscountService.FindByPredicateAsync(p => p.ProductId == productId, includedProperty);

            if (productDiscount == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            ProductDiscountDetailsViewModel model = mapper.Map<ProductDiscountDetailsViewModel>(productDiscount);

            return View(model);
        }

        public async Task<ActionResult> Add(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = await productService.FindByIdAsync(productId);

            if (product == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            BasicProductDataViewModel basicProductDataModel = mapper.Map<BasicProductDataViewModel>(product);
            AddProductDiscountViewModel model = new AddProductDiscountViewModel();
            model.BasicProductDataViewModel = basicProductDataModel;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(AddProductDiscountViewModel model)
        {
            if (ModelState.IsValid)
            {
                ProductDiscount pd = await productDiscountService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);

                if (pd != null)
                {
                    if(pd.Status == ProductDiscountStatus.DuringTime || pd.Status == ProductDiscountStatus.WaitingForStart)
                    {
                        Product product = await productService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
                        BasicProductDataViewModel basicProductDataModel = mapper.Map<BasicProductDataViewModel>(product);
                        model.BasicProductDataViewModel = basicProductDataModel;
                        ModelState.AddModelError("", "Promocja dla tego produktu została juz ustawiona.");
                        return View(model);
                    }
                    else
                    {
                        await productDiscountService.DeleteAsync(pd);
                    }
                }

                ProductDiscount productDiscount = mapper.Map<ProductDiscount>(model);
                productDiscount.ProductId = model.BasicProductDataViewModel.ProductId;
                productDiscount.StartDiscountJobId = jobClient.Schedule(() => productDiscountService.ActivateDiscount(model.BasicProductDataViewModel.ProductId), model.DiscountStartTime);
                productDiscount.StopDiscountJobId = jobClient.Schedule(() => productDiscountService.DisactivateDiscount(model.BasicProductDataViewModel.ProductId), model.DiscountEndTime);
                productDiscount.Status = ProductDiscountStatus.WaitingForStart;

                try
                {
                    await productDiscountService.AddAsync(productDiscount);
                }
                catch
                {
                    jobClient.Delete(productDiscount.StartDiscountJobId);
                    jobClient.Delete(productDiscount.StopDiscountJobId);

                    Product product = await productService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
                    BasicProductDataViewModel basicProductDataModel = mapper.Map<BasicProductDataViewModel>(product);
                    model.BasicProductDataViewModel = basicProductDataModel;
                    ModelState.AddModelError("", "Nie można utworzyć nowej promocji. Proszę spróbować ponownie.");
                    return View(model);
                }
                return RedirectToAction("ProductsOnPromotion", "Product");
            }
            Product prod = await productService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
            BasicProductDataViewModel basicProductModel = mapper.Map<BasicProductDataViewModel>(prod);
            model.BasicProductDataViewModel = basicProductModel;
            return View(model);
        }

        public async Task<JsonResult> GetNewProductPrice(int productId = 0, int discountQuantity = 0)
        {
            if(productId == 0 || discountQuantity == 0)
                return Json(new { success = false, message = "Niekompletne dane! Prosze odświeżyć stronę." }, JsonRequestBehavior.AllowGet);

            decimal newPrice = await productDiscountService.CalculateNewProductPriceAsync(productId, discountQuantity);
            return Json(new { success = true, price = newPrice }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> EditProductDiscount(int productId = 0)
        {
            if(productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] includedProperty = { "Product" };
            ProductDiscount productDiscount = await productDiscountService.FindByPredicateAsync(p => p.ProductId == productId, includedProperty);

            if (productDiscount == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            EditProductDiscountViewModel model = mapper.Map<EditProductDiscountViewModel>(productDiscount);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProductDiscount(EditProductDiscountViewModel model)
        {
            if (ModelState.IsValid)
            {
                ProductDiscount productDiscount = await productDiscountService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
                productDiscount.DiscountQuantity = model.DiscountQuantity;
                productDiscount.DiscountStartTime = model.DiscountStartTime;
                productDiscount.DiscountEndTime = model.DiscountEndTime;

                string oldStartJobId = productDiscount.StartDiscountJobId;
                string oldStopJobId = productDiscount.StopDiscountJobId;

                productDiscount.StartDiscountJobId = jobClient.Schedule(() => productDiscountService.ActivateDiscount(productDiscount.ProductId), model.DiscountStartTime);
                productDiscount.StopDiscountJobId = jobClient.Schedule(() => productDiscountService.DisactivateDiscount(productDiscount.ProductId), model.DiscountEndTime);
                try
                {
                    await productDiscountService.UpdateAsync(productDiscount);
                    jobClient.Delete(oldStartJobId);
                    jobClient.Delete(oldStopJobId);
                }
                catch
                {
                    jobClient.Delete(productDiscount.StartDiscountJobId);
                    jobClient.Delete(productDiscount.StopDiscountJobId);

                    ModelState.AddModelError("", "Błąd edycji promocji. Proszę spróbować ponownie.");
                    Product product = await productService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
                    BasicProductDataViewModel basicProductDataModel = mapper.Map<BasicProductDataViewModel>(product);
                    model.BasicProductDataViewModel = basicProductDataModel;
                    return View(model);
                }
                return RedirectToAction("ProductsOnPromotion", "Product");
            }
            Product prod = await productService.FindByIdAsync(model.BasicProductDataViewModel.ProductId);
            BasicProductDataViewModel basicProductModel = mapper.Map<BasicProductDataViewModel>(prod);
            model.BasicProductDataViewModel = basicProductModel;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<ActionResult> DeleteProductDiscount(int productId = 0)
        {
            if (productId == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ProductDiscount productDiscount = await productDiscountService.FindByIdAsync(productId);
            if (productDiscount == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            await productDiscountService.DeleteAsync(productDiscount);
            jobClient.Delete(productDiscount.StartDiscountJobId);
            jobClient.Delete(productDiscount.StopDiscountJobId);

            return Json(new { success = true, redirectUrl = Url.Action("ProductsOnPromotion", "Product") }, JsonRequestBehavior.AllowGet);
        }
    }
}