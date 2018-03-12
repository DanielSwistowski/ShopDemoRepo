using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class SearchFilterProfile : Profile
    {
        public SearchFilterProfile()
        {
            CreateMap<SearchFilter, SearchFilterViewModel>().ForMember(d => d.Attribute, o => o.MapFrom(s => s.ProductAttribute.Name));
            CreateMap<AddSearchFilterViewModel, SearchFilter>();
        }
    }
}