using Newtonsoft.Json;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogController : Controller
    {
        private readonly ILogFileManagementService logManagement;
        public LogController(ILogFileManagementService logManagement)
        {
            this.logManagement = logManagement;
        }

        public ActionResult Index()
        {
            IEnumerable<string> logFiles = logManagement.GetLogFilesNames();
            List<SelectListItem> filesList = new List<SelectListItem>();
            foreach (var file in logFiles)
            {
                filesList.Add(new SelectListItem { Text = file, Value = file });
            }

            IndexLogViewModel model = new IndexLogViewModel();
            model.Files = filesList;

            return View(model);
        }

        public async Task<ActionResult> ReadLogs(string selectedFile = "")
        {
            if (string.IsNullOrEmpty(selectedFile))
                return Content("Brak danych");

            string jsonFileString = await logManagement.ReadFileAsync(selectedFile);

            if (string.IsNullOrEmpty(jsonFileString))
                return Content("Brak danych");

            IEnumerable<LogFileDetailsViewModel> model = JsonConvert.DeserializeObject<IEnumerable<LogFileDetailsViewModel>>(jsonFileString);

            return PartialView("_LogFilePartial", model.OrderByDescending(d => d.Time));
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public ActionResult DeleteLogFile(string selectedFile = "")
        {
            if (!string.IsNullOrEmpty(selectedFile))
            {
                bool fileExists = logManagement.FileExists(selectedFile);
                if (fileExists)
                {
                    logManagement.DeleteFile(selectedFile);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Wybrany plik nie istnieje" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, message = "Wybierz plik" }, JsonRequestBehavior.AllowGet);
        }
    }
}