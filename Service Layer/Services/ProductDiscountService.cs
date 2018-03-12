using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Threading.Tasks;
using DataAccessLayer;

namespace Service_Layer.Services
{
    public interface IProductDiscountService : IBaseService<ProductDiscount>
    {
        Task<decimal> CalculateNewProductPriceAsync(int productId, int discountQuantity);
        void ActivateDiscount(int discountId);
        void DisactivateDiscount(int discountId);
    }

    public class ProductDiscountService : BaseService<ProductDiscount>, IProductDiscountService
    {
        private readonly IApplicationDbContext context;
        public ProductDiscountService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task<decimal> CalculateNewProductPriceAsync(int productId, int discountQuantity)
        {
            Product product = await context.Products.FindAsync(productId);
            decimal newPrice = product.Price - (product.Price * discountQuantity / 100);
            return newPrice;
        }

        public void ActivateDiscount(int discountId)
        {
            ProductDiscount discount = context.ProductDiscounts.Find(discountId);
            if (discount != null)
            {
                if (discount.Status == ProductDiscountStatus.WaitingForStart)
                    discount.Status = ProductDiscountStatus.DuringTime;

                context.SetModified(discount);
                context.SaveChanges();
            }
        }

        public void DisactivateDiscount(int discountId)
        {
            ProductDiscount discount = context.ProductDiscounts.Find(discountId);
            if (discount != null)
            {
                if (discount.Status == ProductDiscountStatus.DuringTime)
                    discount.Status = ProductDiscountStatus.Ended;

                context.SetModified(discount);
                context.SaveChanges();
            }
        }
    }
}