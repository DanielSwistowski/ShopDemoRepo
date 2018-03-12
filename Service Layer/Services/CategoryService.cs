using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<bool> CategoryExistsAsParentAsync(int? categoryId);
        Task<int?> FindCategoryParentIdAsync(int? categryId);
        Task<IEnumerable<int>> FindProductCategoriesIdsByProductNameAsync(string productName);
        Task<bool> CategoryContainsProductsAsync(int categoryId);
    }

    public class CategoryService : BaseService<Category>, ICategoryService
    {
        private readonly IApplicationDbContext context;
        public CategoryService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task<bool> CategoryContainsProductsAsync(int categoryId)
        {
            return await context.ProductCategory.Where(c => c.CategoryId == categoryId).AnyAsync();
        }

        public async Task<bool> CategoryExistsAsParentAsync(int? categoryId)
        {
            return await context.Categories.Where(p => p.ParentCategoryId == categoryId).AnyAsync();
        }

        public async Task<int?> FindCategoryParentIdAsync(int? categryId)
        {
            return await context.Categories.Where(c => c.CategoryId == categryId).Select(p => p.ParentCategoryId).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<int>> FindProductCategoriesIdsByProductNameAsync(string productName)
        {
            IEnumerable<int> categories = null;
            var searchParams = productName.ToLower().Split(null);
            
            Dictionary<string, int> foundedProductCount = new Dictionary<string, int>();

            foreach (var param in searchParams)
            {
                if (param.Length > 2)
                {
                    foundedProductCount.Add(param, await context.Products.Where(p => p.Name.ToLower().Contains(param)).CountAsync());
                }
            }
            
            if (foundedProductCount.Count > 0)
            {
                string paramName = foundedProductCount.OrderByDescending(i => i.Value).Select(i => i.Key).First();
                int? productId = await context.Products.Where(p => p.Name.ToLower().Contains(paramName)).Select(p => p.ProductId).FirstOrDefaultAsync();
                if (productId != null || productId != default(int))
                {
                    categories = await context.ProductCategory.Where(p => p.ProductId == productId).Select(c => c.CategoryId).ToListAsync();
                }
            }
            return categories;
        }
    }
}