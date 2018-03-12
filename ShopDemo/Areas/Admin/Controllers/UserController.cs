using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Service_Layer.Services;
using AutoMapper;
using ShopDemo.ViewModels;
using PagedList;
using System.Net;
using Postal;
using DataAccessLayer.Models;
using ShopDemo.CustomHelpers;
using Microsoft.AspNet.Identity;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private int PageSize = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TablePageSize"]);
        private readonly IMapper mapper;
        private readonly IEmailService emailService;
        private ApplicationUserManager userManager;
        public UserController(ApplicationUserManager userManager, IMapper mapper, IEmailService emailService)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        // GET: Admin/User
        public async Task<ActionResult> Index([Bind(Prefix = "strona")]int? page, int? pageSize, [Bind(Prefix = "imie")]string searchByFirstName, [Bind(Prefix = "nazwisko")]string searchByLastName, [Bind(Prefix = "email")]string searchByEmail)
        {
            int pageNumber = (page ?? 1);
            ViewBag.FirstName = searchByFirstName;
            ViewBag.LastName = searchByLastName;
            ViewBag.Email = searchByEmail;

            if (TempData["successMessage"] != null)
                ViewBag.SuccessMessage = (string)TempData["successMessage"];

            var users = await userManager.PageAllAsync(pageNumber, PageSize, searchByFirstName, searchByLastName, searchByEmail);

            IEnumerable<UsersListViewModel> usersList = mapper.Map<IEnumerable<UsersListViewModel>>(users.Item1);
            int usersCount = users.Item2;
            var pagedList = new StaticPagedList<UsersListViewModel>(usersList, pageNumber, PageSize, usersCount);
            return View(pagedList);
        }

        public ActionResult SendMessage(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SendEmailViewModel model = new SendEmailViewModel();
            model.EmailTo = email;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMessage(SendEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = new AdminMessageEmail
                {
                    To = model.EmailTo,
                    Subject = EncodeStringHelpers.ConvertStringToUtf8(model.Subject),
                    Message = model.Message,
                };
                await emailService.SendAsync(email);
                TempData["successMessage"] = "Wiadomość do użytkownika " + model.EmailTo + " została wysłana";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<ActionResult> Details(int id = 0)
        {
            if (id == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser user = await userManager.FindByIdAsync(id);

            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            UserDetailsVewModel model = mapper.Map<UserDetailsVewModel>(user);
            model.Roles = await userManager.GetRolesAsync(id);
            Address address = await userManager.GetUserAddressAsync(id);
            model.Address = mapper.Map<UserAddressViewModel>(address);

            return View(model);
        }

        public async Task<ActionResult> LockUserAccount(int id = 0)
        {
            if (id == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser user = await userManager.FindByIdAsync(id);

            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            LockUserAccountViewModel model = mapper.Map<LockUserAccountViewModel>(user);
            model.Subject = "Twoje konto zostało zablokowane";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LockUserAccount(LockUserAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                string loggedUserEmail = User.Identity.GetUserName();
                if (loggedUserEmail == model.Email)
                {
                    ModelState.AddModelError("", "Nie możesz zablokować konta na które jesteś zalogowany!");
                    return View(model);
                }

                await userManager.LockUserAccountAsync(model.Id, model.LockReason);

                var email = new AdminMessageEmail
                {
                    To = model.Email,
                    Subject = EncodeStringHelpers.ConvertStringToUtf8(model.Subject),
                    Message = "Twoje konto zostało zablokowane. Powód blokady: " + model.LockReason
                };
                await emailService.SendAsync(email);

                return RedirectToAction("Details", new { id = model.Id });
            }
            return View(model);
        }

        public async Task<ActionResult> UnlockUserAccount(int id = 0)
        {
            if (id == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ApplicationUser user = await userManager.FindByIdAsync(id);

            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            UnlockUserAccountViewModel model = mapper.Map<UnlockUserAccountViewModel>(user);
            model.Subject = "Twoje konto zostało odblokowane";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnlockUserAccount(UnlockUserAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                await userManager.UnlockUserAccountAsync(model.Id);

                var email = new AdminMessageEmail
                {
                    To = model.Email,
                    Subject = EncodeStringHelpers.ConvertStringToUtf8(model.Subject),
                    Message = model.Message
                };
                await emailService.SendAsync(email);

                return RedirectToAction("Details", new { id = model.Id });
            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (userManager != null)
                {
                    userManager.Dispose();
                    userManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}