using AutoMapper;
using Postal;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService productService;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        public HomeController(IProductService productService, IMapper mapper, IEmailService emailService)
        {
            this.productService = productService;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        public async Task<ActionResult> Index()
        {
            int take = 9;
            var products = await productService.GetTopRatedProductsAsync(take);

            IEnumerable<ProductThumbnailViewModel> model = mapper.Map<IEnumerable<ProductThumbnailViewModel>>(products);

            return View(model);
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(SendEmailToAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                string emailTo = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"];

                var email = new EmailToAdminEmail
                {
                    To = emailTo,
                    From = model.EmailFrom,
                    Subject = EncodeStringHelpers.ConvertStringToUtf8(model.Subject),
                    Message = model.Message,
                };
                await emailService.SendAsync(email);

                return RedirectToAction("MessageSent");
            }
            return View(model);
        }

        public ActionResult MessageSent()
        {
            return View();
        }
    }
}