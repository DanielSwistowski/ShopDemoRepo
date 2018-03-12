using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            CreateMap<ShoppingCart, ShoppingCartViewModel>();

            CreateMap<CartItem, CartItemViewModel>().ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name));
        }
    }
}