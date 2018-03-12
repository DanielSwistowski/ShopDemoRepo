using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryViewModel>();
            CreateMap<AddCategoryViewModel, Category>();
            CreateMap<Category, CategoryDropDownViewModel>();

            CreateMap<Category, CategoryMenuBaseViewModel>()
                .ForMember(c => c.IsSelected, s => s.Ignore())
                .ForMember(c => c.SearchFilter, s => s.Ignore());
        }
    }
}