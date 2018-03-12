using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Web;
using DataAccessLayer.Models;
using System.Threading;
using System.IO;
using System.Linq;

namespace Service_Layer.Services
{
    public interface IPhotoFileManagement
    {
        Task<List<Photo>> AddFilesAsync(IEnumerable<HttpPostedFileBase> files, CancellationToken cancellationToken);
        Task DeleteFileAsync(string photoFileFullPath, string thumbFileFullPath);
        int GetPhotoFilesCount();
        IEnumerable<string> GetPhotoFilesNames();
        void DeleteFile(string fileName);
    }

    public class PhotoFileManagement : IPhotoFileManagement
    {
        private const int PhotoHeight = 1000;
        private const int PhotoWidth = 1000;
        private const int ThumbHeight = 150;
        private const int ThumbWidth = 150;
        private string ProductsGalleryPath = System.Configuration.ConfigurationManager.AppSettings["ProductsGalleryPath"];
        private string ProductsGalleryThumbsPath = System.Configuration.ConfigurationManager.AppSettings["ProductsGalleryThumbPath"];
        private readonly IPathProvider pathProvider;

        public PhotoFileManagement(IPathProvider pathProvider)
        {
            this.pathProvider = pathProvider;
        }

        public async Task<List<Photo>> AddFilesAsync(IEnumerable<HttpPostedFileBase> files, CancellationToken cancellationToken)
        {
            var result = await Task.Run(() => SaveFiles(files), cancellationToken);
            return result;
        }

        public async Task DeleteFileAsync(string photoFileFullPath, string thumbFileFullPath)
        {
            string photoPath = pathProvider.MapPath(photoFileFullPath);
            string thumbPath = pathProvider.MapPath(thumbFileFullPath);
            await Task.Run(() => DeleteImg(photoPath, thumbPath));
        }

        private Photo SaveFile(Image image, string fileExtension)
        {
            var fileName = Guid.NewGuid().ToString();
            string photoPath = pathProvider.MapPath(ProductsGalleryPath + fileName + fileExtension);
            string thumbPath = pathProvider.MapPath(ProductsGalleryThumbsPath + fileName + fileExtension);

            try
            {
                using (var img = ChangeImageSize(image, PhotoWidth, PhotoHeight))
                {
                    img.Save(photoPath, image.RawFormat);
                }
                
                using (var img = ChangeImageSize(image, ThumbWidth, ThumbHeight))
                {
                    img.Save(thumbPath, image.RawFormat);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Photo photo = new Photo();
            photo.PhotoPath = ProductsGalleryPath + fileName + fileExtension;
            photo.PhotoThumbPath = ProductsGalleryThumbsPath + fileName + fileExtension;
            return photo;
        }

        private List<Photo> SaveFiles(IEnumerable<HttpPostedFileBase> files)
        {
            List<Photo> photos = new List<Photo>();
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    using (var img = Image.FromStream(file.InputStream))
                    {
                        Photo photo = SaveFile(img, extension);
                        photos.Add(photo);
                    }
                }
            }
            return photos;
        }

        private static Image ChangeImageSize(Image image, int width, int height)
        {
            var ratioX = (double)width / image.Width;
            var ratioY = (double)height / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        private static void DeleteImg(string photoFileFullPath, string thumbFileFullPath)
        {
            if (File.Exists(photoFileFullPath))
            {
                File.Delete(photoFileFullPath);
            }
            if (File.Exists(thumbFileFullPath))
            {
                File.Delete(thumbFileFullPath);
            }
        }

        public int GetPhotoFilesCount()
        {
            string fullPath = pathProvider.MapPath(ProductsGalleryPath);
            return Directory.GetFiles(fullPath).Length;
        }

        public IEnumerable<string> GetPhotoFilesNames()
        {
            string filesDirectoryFullPath = pathProvider.MapPath(ProductsGalleryPath);
            return Directory.EnumerateFiles(filesDirectoryFullPath).Select(f => Path.GetFileName(f));
        }

        public void DeleteFile(string fileName)
        {
            string photoPath = pathProvider.MapPath(ProductsGalleryPath + fileName);
            string thumbPath = pathProvider.MapPath(ProductsGalleryThumbsPath + fileName);
            DeleteImg(photoPath, thumbPath);
        }
    }
}