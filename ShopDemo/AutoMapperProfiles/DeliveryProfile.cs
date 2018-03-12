using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class DeliveryProfile : Profile
    {
        public DeliveryProfile()
        {
            CreateMap<Delivery, DeliveryOptionsViewModel>();

            CreateMap<PaymentOptions, PaymentOptionsViewModel>();

            CreateMap<Delivery, AdminDeliveryOptionsViewModel>();

            CreateMap<CrudDeliveryViewModel, Delivery>().ReverseMap();
        }
    }
}