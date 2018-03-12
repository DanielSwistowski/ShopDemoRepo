using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;

namespace ShopDemo.AutoMapperProfiles
{
    public class ProductDiscountProfile : Profile
    {
        public ProductDiscountProfile()
        {
            CreateMap<ProductDiscount, ProductDiscountDetailsViewModel>()
                .ForMember(d => d.PromotionPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.BasicProductDataViewModel, o => o.MapFrom(s => s.Product));

            CreateMap<ProductDiscount, AddProductDiscountViewModel>().ReverseMap();

            CreateMap<ProductDiscount, EditProductDiscountViewModel>()
                .ForMember(d => d.PromotionPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d=>d.BasicProductDataViewModel, o=>o.MapFrom(s=>s.Product));

            CreateMap<EditProductDiscountViewModel, ProductDiscount>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.BasicProductDataViewModel.ProductId));

            CreateMap<ProductDiscountStatus, ProductDiscountStatusViewModel>();
        }

        public class ResolveProductPromotionPrice : IValueResolver<ProductDiscount, object, decimal>
        {
            public decimal Resolve(ProductDiscount source, object destination, decimal destMember, ResolutionContext context)
            {
                if (source.Product != null)
                {
                    destMember = source.Product.Price - (source.Product.Price * source.DiscountQuantity / 100);
                }
                else
                {
                    destMember = 0;
                }
                return destMember;
            }
        }
    }
}