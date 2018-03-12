using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Linq;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IProductAttributeService : IBaseService<ProductAttribute>
    {
        Task AddMultipleProductAttributesAsync(IEnumerable<ProductAttribute> productAttributes);
        Task<bool> ProductAttributeExistsAsync(int productAttributeId);
    }

    public class ProductAttributeService : BaseService<ProductAttribute>, IProductAttributeService
    {
        private readonly IApplicationDbContext context;
        public ProductAttributeService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task AddMultipleProductAttributesAsync(IEnumerable<ProductAttribute> productAttributes)
        {
            foreach (var attribute in productAttributes)
            {
                context.ProductAttributes.Add(attribute);
            }
            await context.SaveChangesAsync();
        }

        public async Task<bool> ProductAttributeExistsAsync(int productAttributeId)
        {
            return await context.ProductAttributes.Where(a => a.ProductAttributeId == productAttributeId).AnyAsync();
        }

        public async override Task UpdateAsync(ProductAttribute entity)
        {
            ProductAttribute existingAttribute = await context.ProductAttributes.FindAsync(entity.ProductAttributeId);

            if (existingAttribute.Name != entity.Name)
            {
                var productDetails = await context.ProductCategory.Where(c => c.CategoryId == existingAttribute.CategoryId)
                    .Select(p => p.Product).SelectMany(s => s.ProductDetails).Where(a => a.DetailName == existingAttribute.Name).ToListAsync();

                productDetails.ForEach(d => d.DetailName = entity.Name);
                existingAttribute.Name = entity.Name;
                context.SetModified(existingAttribute);
                await context.SaveChangesAsync();
            }
        }
    }
}