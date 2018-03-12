using DataAccessLayer.Models;
using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin;
using System.Collections.Generic;

namespace ShopDemo.Tests.Areas.Admin
{
    [TestFixture]
    public class HangfireRemovePhotoFilesTest
    {
        Mock<IPhotoFileManagement> mockPhotoFileManagement;
        Mock<IPhotoService> mockPhotoService;

        [SetUp]
        public void SetUp()
        {
            mockPhotoFileManagement = new Mock<IPhotoFileManagement>();
            mockPhotoService = new Mock<IPhotoService>();
        }

        [Test]
        public void RemovePhotoFilesWhichNotExistsIntoDb_removes_files_which_urls_are_not_exists_into_database()
        {
            string ProductsGalleryPath = System.Configuration.ConfigurationManager.AppSettings["ProductsGalleryPath"];

            List<string> filesNamesList = new List<string>() { "file1", "file2", "file3", "file4" };
            mockPhotoFileManagement.Setup(m => m.GetPhotoFilesNames()).Returns(filesNamesList);
            mockPhotoFileManagement.Setup(m => m.GetPhotoFilesCount()).Returns(4);

            List<Photo> photosIntoDb = new List<Photo>();
            photosIntoDb.Add(new Photo { PhotoPath = ProductsGalleryPath + "file1" });
            photosIntoDb.Add(new Photo { PhotoPath = ProductsGalleryPath + "file2" });
            mockPhotoService.Setup(m => m.GetAll()).Returns(photosIntoDb);
            mockPhotoService.Setup(m => m.GetPhotsCount()).Returns(2);

            HangfireRemovePhotoFiles service = new HangfireRemovePhotoFiles(mockPhotoFileManagement.Object, mockPhotoService.Object);
            service.RemovePhotoFilesWhichNotExistsIntoDb();

            mockPhotoFileManagement.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}