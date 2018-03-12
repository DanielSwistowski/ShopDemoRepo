using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ShopDemo.AutoMapperProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, BasicProductDataViewModel>();

            CreateMap<AddProductViewModel, Product>()
                .ForMember(d => d.ProductGallery, o => o.MapFrom(s => s.ProductGallery))
                .ForMember(d => d.ProductDetails, o => o.MapFrom(s => s.ProductDetails));

            CreateMap<Product, EditProductViewModel>()
                .ForMember(d => d.SelectedCategories, o => o.MapFrom(s => s.ProductCategory.Select(c => c.CategoryId)));

            CreateMap<EditProductViewModel, Product>()
                .ForMember(d => d.ProductCategory, o => o.ResolveUsing<ResolveProductCategory>());

            CreateMap<Product, AdminIndexProductViewModel>()
                .ForMember(d => d.PreviewThumbPath, o => o.MapFrom(s => s.ProductGallery.Count == 0 ? string.Empty : s.ProductGallery.FirstOrDefault().PhotoThumbPath))
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.IsInPromotion, o => o.ResolveUsing<ResolveProductPromotionStatus>());

            CreateMap<Product, ProductDeletedFromOfferViewModel>();

            CreateMap<Product, ProductOnPromotionViewModel>()
                .ForMember(d => d.OldPrice, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.DiscountQuantity, o => o.MapFrom(s => s.ProductDiscount.DiscountQuantity))
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.Status, o => o.MapFrom(s => s.ProductDiscount.Status));


            CreateMap<Product, AdminProductDetailsViewModel>()
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.OldPrice, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.IsInPromotion, o => o.ResolveUsing<ResolveProductPromotionStatus>())
                .ForMember(d => d.ProductCategories, o => o.MapFrom(s => s.ProductCategory.Select(c => c.Category)));

            CreateMap<Product, CustomerProductViewModel>()
                .ForMember(d => d.PreviewThumbPath, o => o.MapFrom(s => s.ProductGallery.Count == 0 ? string.Empty : s.ProductGallery.FirstOrDefault().PhotoThumbPath))
                .ForMember(d => d.ProductRate, o => o.ResolveUsing<ResolveProductRate>())
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.OldPrice, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.IsInPromotion, o => o.ResolveUsing<ResolveProductPromotionStatus>());

            CreateMap<Product, CustomerProductDetailsViewModel>()
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.OldPrice, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.IsInPromotion, o => o.ResolveUsing<ResolveProductPromotionStatus>())
                .ForMember(d => d.ProductCategories, o => o.MapFrom(s => s.ProductCategory.Select(c => c.Category)));

            CreateMap<Product, ProductThumbnailViewModel>()
                .ForMember(d => d.PreviewThumbPath, o => o.MapFrom(s => s.ProductGallery.Count == 0 ? string.Empty : s.ProductGallery.FirstOrDefault().PhotoThumbPath))
                .ForMember(d => d.ProductRate, o => o.ResolveUsing<ResolveProductRate>())
                .ForMember(d => d.NewPrice, o => o.ResolveUsing<ResolveProductPromotionPrice>())
                .ForMember(d => d.Description, o => o.ResolveUsing<ResolveProductDescription>())
                .ForMember(d => d.OldPrice, o => o.MapFrom(s => s.Price));
        }

        public class ResolveProductDescription : IValueResolver<Product, object, string>
        {
            public string Resolve(Product source, object destination, string destMember, ResolutionContext context)
            {
                int descriptionLength = 400;
                string description = source.Description;

                if (description.Length > descriptionLength)
                    destMember = description.Substring(0, descriptionLength) + "...";
                else
                    destMember = description;

                return destMember;
            }
        }

        public class ResolveProductPromotionStatus : IValueResolver<Product, object, bool>
        {
            public bool Resolve(Product source, object destination, bool destMember, ResolutionContext context)
            {
                destMember = false;
                if (source.ProductDiscount != null)
                {
                    if (source.ProductDiscount.Status == ProductDiscountStatus.DuringTime)
                        destMember = true;
                }
                else
                {
                    destMember = false;
                }
                return destMember;
            }
        }

        public class ResolveProductPromotionPrice : IValueResolver<Product, object, decimal>
        {
            public decimal Resolve(Product source, object destination, decimal destMember, ResolutionContext context)
            {
                if (source.ProductDiscount != null)
                {
                    if (source.ProductDiscount.Status == ProductDiscountStatus.DuringTime)
                        destMember = source.Price - (source.Price * source.ProductDiscount.DiscountQuantity / 100);
                }
                else
                {
                    destMember = 0;
                }
                return destMember;
            }
        }

        public class ResolveProductRate : IValueResolver<Product, object, double>
        {
            public double Resolve(Product source, object destination, double destMember, ResolutionContext context)
            {
                if (source.ProductRates.Count != 0)
                {
                    destMember = source.ProductRates.Average(p => p.Rate);
                }
                else
                {
                    destMember = 0;
                }
                return destMember;
            }
        }

        public class ResolveProductCategory : IValueResolver<EditProductViewModel, Product, ICollection<ProductCategory>>
        {
            ICollection<ProductCategory> IValueResolver<EditProductViewModel, Product, ICollection<ProductCategory>>.Resolve(EditProductViewModel source, Product destination, ICollection<ProductCategory> destMember, ResolutionContext context)
            {
                destMember = new List<ProductCategory>();
                foreach (var categoryId in source.SelectedCategories)
                {
                    destMember.Add(new ProductCategory { CategoryId = categoryId, ProductId = source.ProductId });
                }
                return destMember;
            }
        }
    }
}