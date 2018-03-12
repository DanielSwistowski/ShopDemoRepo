using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class PhotoControllerTest
    {
        Mock<IPhotoFileManagement> mockPhotoFileManagement;

        [SetUp]
        public void SetUp()
        {
            mockPhotoFileManagement = new Mock<IPhotoFileManagement>();
        }

        [Test]
        public async Task DeletePhotoFile_removes_file()
        {
            mockPhotoFileManagement.Setup(m => m.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            PhotoController photoController = new PhotoController(mockPhotoFileManagement.Object);

            var result = await photoController.DeletePhotoFile(new PhotoViewModel());
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockPhotoFileManagement.Verify(m => m.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DeleteUnsavedGallery_removes_files_belongs_to_unsaved_products()
        {
            mockPhotoFileManagement.Setup(m => m.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            PhotoController photoController = new PhotoController(mockPhotoFileManagement.Object);

            List<PhotoViewModel> photoFiles = new List<PhotoViewModel>();
            photoFiles.Add(new PhotoViewModel());
            photoFiles.Add(new PhotoViewModel());
            photoFiles.Add(new PhotoViewModel());

            var result = await photoController.DeleteUnsavedGallery(photoFiles);
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockPhotoFileManagement.Verify(m => m.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(photoFiles.Count));
        }
    }
}
