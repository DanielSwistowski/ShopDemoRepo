using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Service_Layer.Services;
using ShopDemo.ViewModels;
using DataAccessLayer.Models;
using AutoMapper;

namespace ShopDemo.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager signInManager;
        private ApplicationUserManager userManager;
        private IMapper mapper;

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mapper = mapper;
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Hasło zostało zmienione."
                : message == ManageMessageId.ChangePersonalDataSuccess ? "Dane osobowe zostały zmienione"
                : message == ManageMessageId.ChangeAddressSuccess ? "Adres został zmieniony"
                : message == ManageMessageId.Error ? "Błąd. Prosze odświeżyć stronę i spróbować ponownie."
                : "";

            int userId = User.Identity.GetUserId<int>();
            Address userAddress = await userManager.GetUserAddressAsync(userId);
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            UserPersonalDataViewModel personalDataModel = mapper.Map<UserPersonalDataViewModel>(user);
            personalDataModel.Roles = await userManager.GetRolesAsync(userId);

            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PersonalData = personalDataModel,
                Address = mapper.Map<UserAddressViewModel>(userAddress)
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await userManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await userManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        public async Task<ActionResult> EditPersonalData()
        {
            int userId = User.Identity.GetUserId<int>();
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            EditPersonalDataViewModel model = mapper.Map<EditPersonalDataViewModel>(user);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPersonalData(EditPersonalDataViewModel model)
        {
            if (ModelState.IsValid)
            {
                int userId = User.Identity.GetUserId<int>();
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangePersonalDataSuccess });
                }
                AddErrors(result);
            }
            return View(model);
        }

        public async Task<ActionResult> EditAddress()
        {
            int userId = User.Identity.GetUserId<int>();
            Address userAddress = await userManager.GetUserAddressAsync(userId);
            EditAddressViewModel model = mapper.Map<EditAddressViewModel>(userAddress);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAddress(EditAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                Address address = mapper.Map<Address>(model);
                address.UserId = User.Identity.GetUserId<int>();
                await userManager.UpdateUserAddressAsync(address);
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangeAddressSuccess });
            }
            return View(model);
        }

        public ActionResult AddAddress()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAddress(EditAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                Address address = mapper.Map<Address>(model);
                address.UserId = User.Identity.GetUserId<int>();
                await userManager.UpdateUserAddressAsync(address);
                return RedirectToAction("SelectDeliveryOption", "Delivery");
            }
            return View(model);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && userManager != null)
            {
                userManager.Dispose();
                userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        //private IAuthenticationManager AuthenticationManager
        //{
        //    get
        //    {
        //        return HttpContext.GetOwinContext().Authentication;
        //    }
        //}

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = userManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            ChangePersonalDataSuccess,
            ChangeAddressSuccess,
            Error
        }

        #endregion
    }
}