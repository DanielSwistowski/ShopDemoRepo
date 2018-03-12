using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopDemo.AutoMapperProfiles
{
    public class StatisticsProfile : Profile
    {
        public StatisticsProfile()
        {
            CreateMap<Product, ProductSaleStatisticsViewModel>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.SaleQuantity, o => o.MapFrom(s => s.OrderDetails.Sum(t => t.ProductQuantity)))
                .ForMember(d => d.Total, o => o.MapFrom(s => s.OrderDetails.Sum(t => t.Total)))
                .ForMember(d => d.AverageProductUnitPrice, o => o.ResolveUsing<ResolveAvarageProductPrice>());

            CreateMap<Order, OrderStatisticsViewModel>()
                .ForMember(d => d.OrderedProductCount, o => o.MapFrom(s => s.OrderDetails.Sum(p => p.ProductQuantity)));

            CreateMap<Order, ProductOrderHistoryViewModel>()
                .ForMember(d => d.OrderedProductCount, o => o.Ignore());

            CreateMap<KeyValuePair<ApplicationUser, decimal>, BestCustomersViewModel>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Key.Id))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.Key.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.Key.LastName))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Key.Email))
                .ForMember(d => d.UserOrdersTotalPrice, o => o.MapFrom(s => s.Value));

            CreateMap<KeyValuePair<Delivery, int>, MostPopularDeliveryOptionsViewModel>()
                .ForMember(d => d.Option, o => o.MapFrom(s => s.Key.Option))
                .ForMember(d => d.PaymentOption, o => o.MapFrom(s => s.Key.PaymentOption))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Key.Price))
                .ForMember(d => d.OrdersCount, o => o.MapFrom(s => s.Value));

            CreateMap<SaleSummary, SalesSummaryViewModel>();

            CreateMap<KeyValuePair<Product, Tuple<int,double>>, TopRatedProductsViewModel>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Key.ProductId))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Key.Name))
                .ForMember(d => d.RatesCount, o => o.MapFrom(s => s.Value.Item1))
                .ForMember(d => d.AverageRate, o => o.MapFrom(s => s.Value.Item2));
        }

        public class ResolveAvarageProductPrice : IValueResolver<Product, object, decimal>
        {
            public decimal Resolve(Product source, object destination, decimal destMember, ResolutionContext context)
            {
                return source.OrderDetails.Sum(t => t.Total) / source.OrderDetails.Sum(p => p.ProductQuantity);
            }
        }
    }
}