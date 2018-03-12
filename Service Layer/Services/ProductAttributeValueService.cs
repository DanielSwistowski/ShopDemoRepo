using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IProductAttributeValueService : IBaseService<ProductAttributeValue>
    {
        Task<IEnumerable<ProductAttributeValue>> GetProductAttributeValuesByCategoryIdAndAttributeNameAsync(int categoryId, string attributeName);
        Task AddMultipleProductAttributeValuesAsync(IEnumerable<ProductAttributeValue> productAttributeValues);
    }

    public class ProductAttributeValueService : BaseService<ProductAttributeValue>, IProductAttributeValueService
    {
        private readonly IApplicationDbContext context;
        public ProductAttributeValueService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task AddMultipleProductAttributeValuesAsync(IEnumerable<ProductAttributeValue> productAttributeValues)
        {
            foreach (var attributeValue in productAttributeValues)
            {
                context.ProductAttributeValues.Add(attributeValue);
            }
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductAttributeValue>> GetProductAttributeValuesByCategoryIdAndAttributeNameAsync(int categoryId, string attributeName)
        {
            IEnumerable<ProductAttributeValue> productAttributeValues;
            int productAttributeId = await context.ProductAttributes.Where(p => p.CategoryId == categoryId && p.Name == attributeName).Select(s => s.ProductAttributeId).SingleOrDefaultAsync();
            if (productAttributeId != default(int))
                productAttributeValues = await context.ProductAttributeValues.Where(p => p.ProductAttributeId == productAttributeId).ToListAsync();
            else
                productAttributeValues = new List<ProductAttributeValue>();
            return productAttributeValues;
        }
    }
}
