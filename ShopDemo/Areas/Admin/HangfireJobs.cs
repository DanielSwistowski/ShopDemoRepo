using DataAccessLayer.Models;
using Postal;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopDemo.Areas.Admin
{
    public interface IHangfireAutoCancelOrder
    {
        void CancelNotPaidOrders();
    }

    public class HangfireAutoCancelOrder : IHangfireAutoCancelOrder
    {
        private readonly IOrderService orderService;
        private readonly IEmailService emailService;
        private readonly IDateTimeProvider dateTimeProvider;
        public HangfireAutoCancelOrder(IOrderService orderService, IEmailService emailService, IDateTimeProvider dateTimeProvider)
        {
            this.orderService = orderService;
            this.emailService = emailService;
            this.dateTimeProvider = dateTimeProvider;
        }

        public void CancelNotPaidOrders()
        {
            DateTime checkDate = dateTimeProvider.Now.AddDays(-7);
            IEnumerable<int> ordersIds = orderService.GetAll(o => o.OrderStatus == OrderStatus.PaymentCancelled && o.OrderDate < checkDate).Select(i => i.OrderId);

            foreach (var id in ordersIds)
            {
                string orderOwnerEmail = orderService.CancelNotPaidOrder(id);
                if (!string.IsNullOrEmpty(orderOwnerEmail))
                {
                    var email = new AutoCancelOrderEmail
                    {
                        To = orderOwnerEmail,
                        Subject = EncodeStringHelpers.ConvertStringToUtf8("Zamówienie anulowane"),
                        Message = "Twoje zamówienie zostało anulowane, ponieważ płatność została odrzucona."
                    };
                    emailService.Send(email);
                }
            }
        }
    }

    public interface IHangfireRemovePhotoFiles
    {
        void RemovePhotoFilesWhichNotExistsIntoDb();
    }

    public class HangfireRemovePhotoFiles : IHangfireRemovePhotoFiles
    {
        private readonly IPhotoFileManagement photoFileManagement;
        private readonly IPhotoService photoService;
        public HangfireRemovePhotoFiles(IPhotoFileManagement photoFileManagement, IPhotoService photoService)
        {
            this.photoFileManagement = photoFileManagement;
            this.photoService = photoService;
        }

        public void RemovePhotoFilesWhichNotExistsIntoDb()
        {
            int photoFilesCount = photoFileManagement.GetPhotoFilesCount();
            int photoEntityInDatabaseCount = photoService.GetPhotsCount();

            if (photoFilesCount != photoEntityInDatabaseCount)
            {
                string ProductsGalleryPath = System.Configuration.ConfigurationManager.AppSettings["ProductsGalleryPath"];

                //get all files
                IEnumerable<string> fileNames = photoFileManagement.GetPhotoFilesNames().ToList();

                //get all photos from db
                IEnumerable<Photo> photosInDb = photoService.GetAll().ToList();

                List<string> filesNotExistingInDb = new List<string>();
                foreach (var file in fileNames)
                {
                    string fullPath = ProductsGalleryPath + file;
                    Photo photo = photosInDb.Where(p => p.PhotoPath == fullPath).SingleOrDefault();
                    if (photo == default(Photo))
                        filesNotExistingInDb.Add(file);
                }

                if(filesNotExistingInDb.Count != 0)
                {
                    filesNotExistingInDb.ForEach(f => photoFileManagement.DeleteFile(f));
                }
            }
        }
    }
}