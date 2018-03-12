using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using Service_Layer.BaseService;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IProductService : IBaseService<Product>
    {
        Task<bool> ProductExistsInOrders(int productId);
        Task RemoveProductFromOffer(Product product);
        Task AddProductToOffer(Product product);
        Task<Tuple<IEnumerable<Product>, int>> PageAllAsync(int? pageNumber, int? pageSize, string searchProductByName, decimal? priceFrom, decimal? priceTo, int? categoryId = null, bool isInOffer = false, bool? isOnPromotion = null, bool includeProductRates = false, Dictionary<string, IEnumerable<string>> searchParameters = null);
        Task<int> GetProductCountAsync(int producId);
        Task<int> ActualizeProductQuantityAsync(int productId, int quantity);
        Task<string> GetProductNameByProductIdAsync(int productId);
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int take);
    }

    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly IApplicationDbContext context;
        private readonly IPhotoFileManagement photoFileManagement;
        public ProductService(IApplicationDbContext ctx, IPhotoFileManagement photoFileManagement) : base(ctx)
        {
            context = ctx;
            this.photoFileManagement = photoFileManagement;
        }

        public override async Task UpdateAsync(Product entity)
        {
            Product product = await context.Products.FindAsync(entity.ProductId);
            product.Name = entity.Name;
            product.IsInOffer = entity.IsInOffer;
            product.DeletedFromOfferDate = entity.DeletedFromOfferDate;
            product.Description = entity.Description;
            product.CreatedAt = entity.CreatedAt;
            product.Price = entity.Price;
            product.Quantity = entity.Quantity;

            //update product category
            List<ProductCategory> productCategories = await context.ProductCategory.Where(p => p.ProductId == entity.ProductId).ToListAsync();
            foreach (var productCategory in productCategories)
            {
                if (!entity.ProductCategory.Where(c => c.CategoryId == productCategory.CategoryId).Any())
                {
                    context.ProductCategory.Remove(productCategory);
                }
            }

            foreach (var category in entity.ProductCategory)
            {
                if (!productCategories.Where(c => c.CategoryId == category.CategoryId).Any())
                {
                    ProductCategory productCategory = new ProductCategory();
                    productCategory.CategoryId = category.CategoryId;
                    productCategory.ProductId = entity.ProductId;
                    context.ProductCategory.Add(productCategory);
                }
            }

            //update product details
            List<ProductDetail> productDetails = await context.ProductDetails.Where(p => p.ProductId == entity.ProductId).ToListAsync();
            foreach (var productDetail in productDetails)
            {
                if (!entity.ProductDetails.Where(p => p.DetailName == productDetail.DetailName).Any())
                {
                    context.ProductDetails.Remove(productDetail);
                }
            }
            foreach (var detail in entity.ProductDetails)
            {
                if (!productDetails.Where(p => p.DetailName == detail.DetailName).Any())
                {
                    ProductDetail productDetail = new ProductDetail();
                    productDetail.DetailName = detail.DetailName;
                    productDetail.DetailValue = detail.DetailValue;
                    productDetail.ProductId = entity.ProductId;
                    context.ProductDetails.Add(productDetail);
                }
                else
                {
                    ProductDetail productDetail = productDetails.Where(p => p.DetailName == detail.DetailName).Single();
                    productDetail.DetailValue = detail.DetailValue;
                    context.SetModified(productDetail);
                }
            }

            //update product gallery
            List<Photo> filesToRemove = new List<Photo>();
            List<Photo> productGallery = await context.Photos.Where(p => p.ProductId == entity.ProductId).ToListAsync();
            foreach (var photo in productGallery)
            {
                if (!entity.ProductGallery.Where(p => p.PhotoPath == photo.PhotoPath).Any())
                {
                    context.Photos.Remove(photo);
                    filesToRemove.Add(photo);
                }
            }

            foreach (var photo in entity.ProductGallery)
            {
                if (!productGallery.Where(p => p.PhotoPath == photo.PhotoPath).Any())
                {
                    Photo p = new Photo();
                    p.PhotoPath = photo.PhotoPath;
                    p.PhotoThumbPath = photo.PhotoThumbPath;
                    p.ProductId = entity.ProductId;
                    context.Photos.Add(p);
                }
            }

            context.SetModified(product);

            await context.SaveChangesAsync();

            foreach (var photoFile in filesToRemove)
            {
                await photoFileManagement.DeleteFileAsync(photoFile.PhotoPath, photoFile.PhotoThumbPath);
            }
        }

        public async override Task DeleteAsync(Product entity)
        {
            IEnumerable<Photo> productGallery = await context.Photos.Where(p => p.ProductId == entity.ProductId).ToListAsync();
            context.Products.Remove(entity);
            await context.SaveChangesAsync();

            foreach (var photo in productGallery)
            {
                await photoFileManagement.DeleteFileAsync(photo.PhotoPath, photo.PhotoThumbPath);
            }
        }

        public async Task AddProductToOffer(Product product)
        {
            product.IsInOffer = true;
            product.DeletedFromOfferDate = null;
            context.SetModified(product);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ProductExistsInOrders(int productId)
        {
            return await context.OrderDetails.Where(p => p.ProductId == productId).AnyAsync();
        }

        public async Task RemoveProductFromOffer(Product product)
        {
            product.IsInOffer = false;
            product.DeletedFromOfferDate = DateTime.Now;
            context.SetModified(product);
            await context.SaveChangesAsync();
        }

        public async Task<Tuple<IEnumerable<Product>, int>> PageAllAsync(int? pageNumber, int? pageSize, string searchProductByName, decimal? priceFrom, decimal? priceTo, int? categoryId = null, bool isInOffer = false, bool? isOnPromotion = null, bool includeProductRates = false, Dictionary<string, IEnumerable<string>> searchParameters = null)
        {
            IQueryable<Product> query = null;

            if (categoryId != null)
                query = context.ProductCategory.Where(c => c.CategoryId == categoryId).Select(p => p.Product).Include(p => p.ProductGallery);
            else
                query = context.Products.Include(p => p.ProductGallery);

            if (isInOffer != false)
                query = query.Where(p => p.IsInOffer == true);
            else
                query = query.Where(p => p.IsInOffer == false);

            if (isOnPromotion == null)
            {
                query = query.Include(p => p.ProductDiscount);
            }
            else
            {
                if (isOnPromotion != false)
                    query = query.Include(p => p.ProductDiscount).Where(p => p.ProductDiscount != null);
                else
                    query = query.Where(p => p.ProductDiscount.Status != ProductDiscountStatus.DuringTime);
            }

            if (includeProductRates == true)
                query = query.Include(p => p.ProductRates);

            if (priceFrom != null)
                query = query.Where(p => p.Price >= priceFrom);

            if (priceTo != null)
                query = query.Where(p => p.Price <= priceTo);

            if (!string.IsNullOrEmpty(searchProductByName))
                query = query.Where(p => p.Name.ToLower().Contains(searchProductByName.ToLower()));

            if (searchParameters != null)
            {
                query = query.Include(p => p.ProductDetails);
                var predicate = PredicateBuildier.PredicateBuilder.SearchByDetails(searchParameters);
                query = query.Where(predicate);
            }

            int productsCount = await query.CountAsync();

            if (pageNumber != null && pageSize != null)
            {
                query = query.OrderBy(p => p.ProductId);
                query = query.Skip(((int)pageNumber - 1) * (int)pageSize);
                query = query.Take((int)pageSize);
            }

            IEnumerable<Product> products = await query.ToListAsync();
            return Tuple.Create(products, productsCount);
        }

        public async Task<int> GetProductCountAsync(int producId)
        {
            Product product = await context.Products.FindAsync(producId);
            if (product != null)
                return product.Quantity;
            else
                return 0;
        }

        public async Task<int> ActualizeProductQuantityAsync(int productId, int quantity)
        {
            Product product = await context.Products.FindAsync(productId);
            int newProductQuantity = product.Quantity + quantity;
            product.Quantity = newProductQuantity;
            context.SetModified(product);
            await context.SaveChangesAsync();
            return newProductQuantity;
        }

        public async Task<string> GetProductNameByProductIdAsync(int productId)
        {
            Product product = await context.Products.FindAsync(productId);
            return product != null ? product.Name : null;
        }

        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int take)
        {
            return await context.Products.Include(p => p.ProductRates).Include(p => p.ProductDiscount).Include(p => p.ProductGallery)
                .Where(p=>p.IsInOffer == true).OrderByDescending(o => o.ProductRates.Average(r => r.Rate)).ThenByDescending(o => o.ProductRates.Count).Take(take).ToListAsync();
        }
    }
}