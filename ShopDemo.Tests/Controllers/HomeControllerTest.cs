using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ShopDemo.Controllers;
using NUnit.Framework;
using ShopDemo.ViewModels;
using ShopDemo.CustomValidationAttributes;
using System.Threading.Tasks;
using Moq;
using Service_Layer.Services;
using Postal;
using AutoMapper;
using ShopDemo.AutoMapperProfiles;
using DataAccessLayer.Models;

namespace ShopDemo.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTest
    {
        Mock<IProductService> mockProductService;
        Mock<IEmailService> mockEmailService;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockProductService = new Mock<IProductService>();
            mockEmailService = new Mock<IEmailService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductProfile());
            });
            mapper = config.CreateMapper();
        }

        [Test]
        public async Task Index_returns_view_with_top_rated_products()
        {
            List<ProductRate> rates1 = new List<ProductRate>();
            rates1.Add(new ProductRate { ProductId=1, Rate = 2 });
            rates1.Add(new ProductRate { ProductId = 1, Rate = 4 });
            Product p1 = new Product { ProductId=1, Name="Product1", Price=20, Description="Desc1", ProductRates = rates1 };

            List<ProductRate> rates2 = new List<ProductRate>();
            rates2.Add(new ProductRate { ProductId = 2, Rate = 1 });
            rates2.Add(new ProductRate { ProductId = 2, Rate = 5 });
            Product p2 = new Product { ProductId = 2, Name = "Product2", Price = 10, Description = "Desc2", ProductRates = rates2 };

            List<ProductRate> rates3 = new List<ProductRate>();
            rates3.Add(new ProductRate { ProductId = 3, Rate = 1 });
            rates3.Add(new ProductRate { ProductId = 3, Rate = 3 });
            Product p3 = new Product { ProductId = 3, Name = "Product3", Price = 30, Description = "Desc3", ProductRates = rates3 };

            List<Product> products = new List<Product>();
            products.Add(p1);
            products.Add(p2);
            products.Add(p3);

            mockProductService.Setup(m => m.GetTopRatedProductsAsync(It.IsAny<int>())).ReturnsAsync(products);

            HomeController controller = new HomeController(mockProductService.Object, mapper, mockEmailService.Object);

            var result = await controller.Index() as ViewResult;
            var resultModel = ((IEnumerable<ProductThumbnailViewModel>)result.Model).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, resultModel.Count);
            Assert.AreEqual("Product1", resultModel[0].Name);
            Assert.AreEqual("Product2", resultModel[1].Name);
            Assert.AreEqual("Product3", resultModel[2].Name);
        }

        [Test]
        public async Task Contact_returns_model_state_error_if_model_state_is_not_valid()
        {
            HomeController controller = new HomeController(mockProductService.Object, mapper, mockEmailService.Object);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.Contact(It.IsAny<SendEmailToAdminViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task Contact_sent_email_and_redirects_to_MessageSent()
        {
            HomeController controller = new HomeController(mockProductService.Object, mapper, mockEmailService.Object);

            var result = await controller.Contact(new SendEmailToAdminViewModel { EmailFrom="email@gmail.com", Message="Message", Subject="Subject" }) as RedirectToRouteResult;

            Assert.AreEqual("MessageSent", result.RouteValues["action"]);
            mockEmailService.Verify(m => m.SendAsync(It.IsAny<EmailToAdminEmail>()), Times.Once);
        }
    }
}
