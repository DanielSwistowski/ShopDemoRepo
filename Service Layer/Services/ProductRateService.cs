using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IProductRateService : IBaseService<ProductRate>
    {
        Task<bool> NickNameIsAvailableForCurrentProduct(int productId, string nickName);
    }

    public class ProductRateService : BaseService<ProductRate>, IProductRateService
    {
        private readonly IApplicationDbContext context;
        public ProductRateService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task<bool> NickNameIsAvailableForCurrentProduct(int productId, string nickName)
        {
            return !await context.ProductRates.Where(p => p.ProductId == productId).AnyAsync(n => n.NickName == nickName);
        }
    }
}
