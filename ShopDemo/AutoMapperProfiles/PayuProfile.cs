using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class PayuProfile : Profile
    {
        public PayuProfile()
        {
            CreateMap<OrderDetails, DataAccessLayer.Models.Payu.Product>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.ProductQuantity))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => (int)(s.ProductUnitPrice * 100)));

            CreateMap<ApplicationUser, DataAccessLayer.Models.Payu.Buyer>();
        }
    }
}