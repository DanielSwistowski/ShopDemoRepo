using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class ProductAttributeProfile : Profile
    {
        public ProductAttributeProfile()
        {
            CreateMap<ProductAttribute, ProductAttributeViewModel>();

            CreateMap<ProductAttribute, ProductAttributeWithValuesViewModel>()
                .ForMember(d => d.AttributeValues, o => o.MapFrom(s => s.ProductAttributeValues.Count == 0 ? null : s.ProductAttributeValues.Select(p => p.AttributeValue)));
        }
    }
}