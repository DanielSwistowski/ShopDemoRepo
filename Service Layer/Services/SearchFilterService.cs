using DataAccessLayer.Models;
using Service_Layer.BaseService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Data.Entity;

namespace Service_Layer.Services
{
    public interface ISearchFilterService : IBaseService<SearchFilter>
    {
        Task<bool> SearchFilterExistsAsync(int productAttributeId);
        Task<Dictionary<string, IEnumerable<string>>> RemoveUnexistingParametesFromFiltrAsync(int? categoryId, Dictionary<string, IEnumerable<string>> filtr);
    }

    public class SearchFilterService : BaseService<SearchFilter>, ISearchFilterService
    {
        private readonly IApplicationDbContext context;

        public SearchFilterService(IApplicationDbContext ctx) : base(ctx)
        {
            context = ctx;
        }

        public async Task<bool> SearchFilterExistsAsync(int productAttributeId)
        {
            return await context.SearchFilters.AnyAsync(s => s.ProductAttributeId == productAttributeId);
        }

        public async Task<Dictionary<string, IEnumerable<string>>> RemoveUnexistingParametesFromFiltrAsync(int? categoryId, Dictionary<string, IEnumerable<string>> filtr)
        {
            Dictionary<string, IEnumerable<string>> newSearchParams = null;

            if (filtr != null)
            {
                newSearchParams = new Dictionary<string, IEnumerable<string>>();

                var existingParameters = await context.ProductAttributes.Include(p => p.ProductAttributeValues).Include(s => s.SearchFilter).Where(c => c.CategoryId == categoryId && c.SearchFilter != null).ToListAsync();

                var filtrParams = filtr.Keys;

                foreach (var param in filtrParams)
                {
                    if (existingParameters.Select(a => a.Name.ToLower()).Contains(param))
                    {
                        var filtrParamValues = filtr[param];

                        List<string> newParamValues = new List<string>();

                        var existingParameterValues = existingParameters.Where(p => p.Name.ToLower() == param).Select(a => a.ProductAttributeValues).Single();

                        foreach (var value in filtrParamValues)
                        {
                            if (existingParameterValues.Select(p => p.AttributeValue.ToLower()).Contains(value))
                            {
                                newParamValues.Add(value);
                            }
                        }
                        if(newParamValues.Count > 0)
                            newSearchParams.Add(param, newParamValues);
                    }
                }
            }

            return newSearchParams;
        }
    }
}