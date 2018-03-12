using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class ProductAttributeValueProfile : Profile
    {
        public ProductAttributeValueProfile()
        {
            CreateMap<ProductAttributeValue, ProductAttributeValueViewModel>();
            CreateMap<AddProductAttributeValueViewModel, ProductAttributeValue>();
        }
    }
}