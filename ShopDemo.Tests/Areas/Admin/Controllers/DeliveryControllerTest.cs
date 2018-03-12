using AutoMapper;
using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.AutoMapperProfiles;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class DeliveryControllerTest
    {
        Mock<IDeliveryService> mockDeliveryService;
        IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            mockDeliveryService = new Mock<IDeliveryService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DeliveryProfile());
            });
            mapper = config.CreateMapper();
        }

        [Test]
        public async Task Index_returns_view_with_delivery_options()
        {
            List<Delivery> deliveryOptions = new List<Delivery>();
            deliveryOptions.Add(new Delivery { DeliveryId = 1, IsActive = true, Option = "Option1", PaymentOption = PaymentOptions.NotApplicable, Price = 0, RealizationTimeInDays = 0 });
            deliveryOptions.Add(new Delivery { DeliveryId = 2, IsActive = true, Option = "Option2", PaymentOption = PaymentOptions.CashOnDelivery, Price = 10, RealizationTimeInDays = 1 });
            deliveryOptions.Add(new Delivery { DeliveryId = 3, IsActive = true, Option = "Option3", PaymentOption = PaymentOptions.PaymentByTransfer, Price = 15, RealizationTimeInDays = 2 });
            deliveryOptions.Add(new Delivery { DeliveryId = 4, IsActive = false, Option = "Option4", PaymentOption = PaymentOptions.PaymentByTransfer, Price = 20, RealizationTimeInDays = 1 });

            mockDeliveryService.Setup(m => m.GetAllAsync()).ReturnsAsync(deliveryOptions);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.Index() as ViewResult;
            var resultModel = ((IEnumerable<AdminDeliveryOptionsViewModel>)result.Model).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, resultModel.Count());
            Assert.AreEqual("Option1", resultModel[0].Option);
            Assert.AreEqual(PaymentOptionsViewModel.CashOnDelivery, resultModel[1].PaymentOption);
            Assert.AreEqual(15, resultModel[2].Price);
            Assert.AreEqual(false, resultModel[3].IsActive);
        }

        #region AddNewDelivery
        [Test]
        public async Task AddNewDelivery_returns_model_state_error_if_model_state_is_not_valid()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);
            controller.ModelState.Clear();
            controller.ModelState.AddModelError("", "Errror");

            var result = await controller.AddNewDelivery(It.IsAny<CrudDeliveryViewModel>()) as ViewResult;

            Assert.IsTrue(result.ViewData.ModelState.Count == 1);
        }

        [Test]
        public async Task AddNewDelivery_adds_new_delivery_and_redirects_to_Index_action()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.AddNewDelivery(It.IsAny<CrudDeliveryViewModel>()) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDeliveryService.Verify(m => m.AddAsync(It.IsAny<Delivery>()), Times.Once);
        }
        #endregion

        #region DeleteDelivery
        [Test]
        public async Task DeleteDelivery_returns_BadRequest_if_deliveryId_is_not_provided()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDelivery() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteDelivery_returns_NotFound_if_delivery_is_null()
        {
            Delivery nullDelivery = null;
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullDelivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDelivery(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeleteDelivery_pass_error_message_form_TempData_to_ViewBag_if_TempData_is_not_null()
        {
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Delivery());
            var temp = new TempDataDictionary();
            temp.Add("errorMessage", "Error");

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);
            controller.TempData = temp;

            var result = await controller.DeleteDelivery(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewBag.ErrorMessage);
        }

        [Test]
        public async Task DeleteDelivery_returns_view_with_delivery_model()
        {
            Delivery delivery = new Delivery { DeliveryId = 1, IsActive = true, Option = "Option1", PaymentOption = PaymentOptions.NotApplicable, Price = 0, RealizationTimeInDays = 0 };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDelivery(10) as ViewResult;
            var resultModel = (CrudDeliveryViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(true, resultModel.IsActive);
            Assert.AreEqual("Option1", resultModel.Option);
            Assert.AreEqual(PaymentOptionsViewModel.NotApplicable, resultModel.PaymentOption);
            Assert.AreEqual(0, resultModel.Price);
            Assert.AreEqual(0, resultModel.RealizationTimeInDays);
        }
        #endregion

        #region DeleteDeliveryConfirm
        [Test]
        public async Task DeleteDeliveryConfirm_redirects_to_DeactivateDeliveryOption_action_if_delivery_exists_on_orders_list_and_is_active()
        {
            Delivery delivery = new Delivery { IsActive = true };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);
            mockDeliveryService.Setup(m => m.DeliveryExistsInOrdersAsync(It.IsAny<int>())).ReturnsAsync(true);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDeliveryConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("DeactivateDeliveryOption", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["deliveryId"]);
        }

        [Test]
        public async Task DeleteDeliveryConfirm_redirects_to_DeleteDelivery_action_if_delivery_exists_on_orders_list_and_is_not_active()
        {
            Delivery delivery = new Delivery { IsActive = false };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);
            mockDeliveryService.Setup(m => m.DeliveryExistsInOrdersAsync(It.IsAny<int>())).ReturnsAsync(true);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDeliveryConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("DeleteDelivery", result.RouteValues["action"]);
            Assert.AreEqual(10, result.RouteValues["deliveryId"]);
        }

        [Test]
        public async Task DeleteDeliveryConfirm_removes_delivery_option_and_redirects_to_Index_action()
        {
            mockDeliveryService.Setup(m => m.DeliveryExistsInOrdersAsync(It.IsAny<int>())).ReturnsAsync(false);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeleteDeliveryConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDeliveryService.Verify(m => m.DeleteAsync(It.IsAny<int>()), Times.Once);
        }
        #endregion

        #region DeactivateDeliveryOption
        [Test]
        public async Task DeactivateDeliveryOption_returns_BadRequest_if_deliveryId_is_not_provided()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeactivateDeliveryOption() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeactivateDeliveryOption_returns_NotFound_if_delivery_is_null()
        {
            Delivery nullDelivery = null;
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullDelivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeactivateDeliveryOption(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeactivateDeliveryOption_pass_error_message_form_TempData_to_ViewBag_if_TempData_is_not_null()
        {
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(new Delivery());
            var temp = new TempDataDictionary();
            temp.Add("errorMessage", "Error");

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);
            controller.TempData = temp;

            var result = await controller.DeactivateDeliveryOption(10) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewBag.ErrorMessage);
        }

        [Test]
        public async Task DeactivateDeliveryOption_returns_view_with_delivery_model()
        {
            Delivery delivery = new Delivery { DeliveryId = 1, IsActive = true, Option = "Option1", PaymentOption = PaymentOptions.NotApplicable, Price = 0, RealizationTimeInDays = 0 };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeactivateDeliveryOption(10) as ViewResult;
            var resultModel = (CrudDeliveryViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(true, resultModel.IsActive);
            Assert.AreEqual("Option1", resultModel.Option);
            Assert.AreEqual(PaymentOptionsViewModel.NotApplicable, resultModel.PaymentOption);
            Assert.AreEqual(0, resultModel.Price);
            Assert.AreEqual(0, resultModel.RealizationTimeInDays);
        }
        #endregion

        [Test]
        public async Task DeactivateDeliveryOptionConfirm_set_delivery_IsActive_to_false_and_redirects_to_Index_action()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.DeactivateDeliveryOptionConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDeliveryService.Verify(m => m.DeactivateDeliveryOptionAsync(It.IsAny<int>()), Times.Once);
        }

        #region ActivateDeliveryOption
        [Test]
        public async Task ActivateDeliveryOption_returns_BadRequest_if_deliveryId_is_not_provided()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.ActivateDeliveryOption() as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task ActivateDeliveryOption_returns_NotFound_if_delivery_is_null()
        {
            Delivery nullDelivery = null;
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(nullDelivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.ActivateDeliveryOption(10) as HttpStatusCodeResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task ActivateDeliveryOption_returns_view_with_delivery_model()
        {
            Delivery delivery = new Delivery { DeliveryId = 1, IsActive = true, Option = "Option1", PaymentOption = PaymentOptions.NotApplicable, Price = 0, RealizationTimeInDays = 0 };
            mockDeliveryService.Setup(m => m.FindByIdAsync(It.IsAny<int>())).ReturnsAsync(delivery);

            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.ActivateDeliveryOption(10) as ViewResult;
            var resultModel = (CrudDeliveryViewModel)result.Model;

            Assert.IsNotNull(result);
            Assert.AreEqual(true, resultModel.IsActive);
            Assert.AreEqual("Option1", resultModel.Option);
            Assert.AreEqual(PaymentOptionsViewModel.NotApplicable, resultModel.PaymentOption);
            Assert.AreEqual(0, resultModel.Price);
            Assert.AreEqual(0, resultModel.RealizationTimeInDays);
        }
        #endregion

        [Test]
        public async Task ActivateDeliveryOptionConfirm_set_delivery_IsActive_to_true_and_redirects_to_Index_action()
        {
            DeliveryController controller = new DeliveryController(mockDeliveryService.Object, mapper);

            var result = await controller.ActivateDeliveryOptionConfirm(10) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            mockDeliveryService.Verify(m => m.ActivateDeliveryOptionAsync(It.IsAny<int>()), Times.Once);
        }
    }
}
