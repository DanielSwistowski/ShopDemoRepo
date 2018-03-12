using DataAccessLayer.Models;
using NUnit.Framework;
using Service_Layer.Services;

namespace Service_Layer.Tests.Services
{
    [TestFixture]
    public class PhotoServiceTest
    {
        PhotoService service;

        [SetUp]
        public void SetUp()
        {
            FakeDbContext context = new FakeDbContext();
            context.Photos.Add(new Photo());
            context.Photos.Add(new Photo());
            context.Photos.Add(new Photo());
            context.Photos.Add(new Photo());
            context.Photos.Add(new Photo());

            service = new PhotoService(context);
        }

        [Test]
        public void GetPhotsCount_returns_correct_photos_count()
        {
            var result = service.GetPhotsCount();

            Assert.AreEqual(5, result);
        }
    }
}
