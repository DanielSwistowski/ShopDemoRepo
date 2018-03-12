using DataAccessLayer.Models;
using Service_Layer.BaseService;
using DataAccessLayer;
using System.Threading.Tasks;
using System.Linq;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface IDeliveryService : IBaseService<Delivery>
    {
        Task<bool> DeliveryExistsInOrdersAsync(int deliveryId);
        Task DeactivateDeliveryOptionAsync(int deliveryId);
        Task ActivateDeliveryOptionAsync(int deliveryId);
    }

    public class DeliveryService : BaseService<Delivery>, IDeliveryService
    {
        private readonly IApplicationDbContext context;
        public DeliveryService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task<bool> DeliveryExistsInOrdersAsync(int deliveryId)
        {
            return await context.Orders.Where(d => d.DeliveryId == deliveryId).AnyAsync();
        }

        public async Task ActivateDeliveryOptionAsync(int deliveryId)
        {
            Delivery delivery = await context.Deliveries.FindAsync(deliveryId);
            delivery.IsActive = true;
            context.SetModified(delivery);
            await context.SaveChangesAsync();
        }

        public async Task DeactivateDeliveryOptionAsync(int deliveryId)
        {
            Delivery delivery = await context.Deliveries.FindAsync(deliveryId);
            delivery.IsActive = false;
            context.SetModified(delivery);
            await context.SaveChangesAsync();
        }
    }
}
