using DataAccessLayer.Models;
using Service_Layer.BaseService;
using DataAccessLayer;
using System.Linq;

namespace Service_Layer.Services
{
    public interface IPhotoService : IBaseService<Photo>
    {
        int GetPhotsCount();
    }

    public class PhotoService : BaseService<Photo>, IPhotoService
    {
        private readonly IApplicationDbContext context;
        public PhotoService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public int GetPhotsCount()
        {
            return context.Photos.Count();
        }
    }
}
