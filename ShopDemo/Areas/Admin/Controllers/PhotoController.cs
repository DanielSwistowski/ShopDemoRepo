using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomValidationAttributes;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PhotoController : Controller
    {
        private readonly IPhotoFileManagement photoFileManagement;

        public PhotoController(IPhotoFileManagement photoFileManagement)
        {
            this.photoFileManagement = photoFileManagement;
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> SavePhotoFile(CancellationToken cancellationToken)
        {
            CancellationToken disconnectedToken = Response.ClientDisconnectedToken;
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, disconnectedToken);

            List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
            foreach (string file in Request.Files)
            {
                files.Add(Request.Files[file]);
            }
            List<Photo> photos = await photoFileManagement.AddFilesAsync(files, cancellationToken);

            return Json(photos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> DeletePhotoFile(PhotoViewModel model)
        {
            await photoFileManagement.DeleteFileAsync(model.PhotoPath, model.PhotoThumbPath);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenForAjaxPost]
        public async Task<JsonResult> DeleteUnsavedGallery(IEnumerable<PhotoViewModel> photos)
        {
            foreach (var photo in photos)
            {
                await photoFileManagement.DeleteFileAsync(photo.PhotoPath, photo.PhotoThumbPath);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}