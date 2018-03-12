using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopDemo.AutoMapperProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<CartItem, OrderDetails>()
                .ForMember(d => d.ProductQuantity, o => o.MapFrom(s => s.ProductCount))
                .ForMember(d => d.Total, o => o.MapFrom(s => s.ProductTotalPrice))
                .ForMember(d => d.ProductUnitPrice, o => o.MapFrom(s => s.ProductPrice))
                .ForMember(d => d.Product, o => o.Ignore());

            CreateMap<CartItem, OrderFailureViewModel>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Error, o => o.Ignore());

            CreateMap<Order, OrdersIndexViewModel>();

            CreateMap<OrderStatus, OrderStatusViewModel>();

            CreateMap<Order, OrderDetailViewModel>()
                .ForMember(d => d.CustomerData, o => o.MapFrom(s => s.User))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.User.Address))
                .ForMember(d => d.OrderDetails, o => o.ResolveUsing<ResolveOrderDetails>());

            CreateMap<Order, RealizeOrderViewModel>()
                .ForMember(d => d.Delivery, o => o.MapFrom(s => s.DeliveryOption))
                .ForMember(d => d.OrderDetails, o => o.MapFrom(s => s.OrderDetails));

            CreateMap<Order, UncompleteOrdersViewModel>();

            CreateMap<Order, OrderBillViewModel>()
                .ForMember(d => d.OrderDetails, o => o.ResolveUsing<ResolveOrderDetails>())
                .ForMember(d => d.Customer, o => o.MapFrom(s => s.User));

            CreateMap<Order, AdminCancelOrder>()
                .ForMember(d => d.PaymentOption, o => o.MapFrom(s => s.DeliveryOption.PaymentOption))
                .ForMember(d => d.Message, o => o.Ignore());

            CreateMap<Order, UserOrdersListViewModel>();
        }

        public class ResolveOrderDetails : IValueResolver<Order, object, List<OrderDetailsViewModel>>
        {
            public List<OrderDetailsViewModel> Resolve(Order source, object destination, List<OrderDetailsViewModel> destMember, ResolutionContext context)
            {
                List<OrderDetailsViewModel> orderDetails = new List<OrderDetailsViewModel>();
                foreach (var item in source.OrderDetails)
                {
                    orderDetails.Add(new OrderDetailsViewModel
                    {
                        ProductId = item.Product.ProductId,
                        ProductName = item.Product.Name,
                        ProductQuantity = item.ProductQuantity,
                        ProductUnitPrice = item.ProductUnitPrice,
                        Total = item.Total
                    });
                }
                if (orderDetails.Count > 0)
                    orderDetails.Add(new OrderDetailsViewModel
                    {
                        ProductId = 0,
                        ProductName = source.DeliveryOption.Option,
                        ProductQuantity = 1,
                        ProductUnitPrice = source.DeliveryOption.Price,
                        Total = source.DeliveryOption.Price
                    });
                return orderDetails;
            }
        }
    }
}