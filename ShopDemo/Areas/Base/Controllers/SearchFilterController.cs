using AutoMapper;
using DataAccessLayer.Models;
using Service_Layer.Services;
using ShopDemo.CustomHelpers;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Areas.Base.Controllers
{
    public class SearchFilterController : Controller
    {
        private readonly ISearchFilterService searchFilterService;
        private readonly IMapper mapper;

        public SearchFilterController(ISearchFilterService searchFilterService, IMapper mapper)
        {
            this.searchFilterService = searchFilterService;
            this.mapper = mapper;
        }

        public async Task<PartialViewResult> GetProductSearchFiltersPartial(int? categoryId, string currentSelectedParams)
        {
            string[] includedProperty = { "ProductAttribute" };
            var searchFilters = await searchFilterService.GetAllAsync(c => c.CategoryId == categoryId, includedProperty);

            List<SearchFiltersForProductViewModel> searchFiltersModel = new List<SearchFiltersForProductViewModel>();
            foreach (var filter in searchFilters)
            {
                searchFiltersModel.Add(new SearchFiltersForProductViewModel
                {
                    Attribute = filter.ProductAttribute.Name,
                    FilterType = filter.FilterType,
                    AttributeValues = GetFilterValues(filter.ProductAttribute.Name, filter.ProductAttribute.ProductAttributeValues, currentSelectedParams)
                });
            }

            return PartialView("_ProductSearchFiltersPartial", searchFiltersModel);
        }

        private List<AttributeValueForSearchFilterViewModel> GetFilterValues(string attribueName, IEnumerable<ProductAttributeValue> allAttributeValues, string selectedValues)
        {
            List<AttributeValueForSearchFilterViewModel> valuesList = new List<AttributeValueForSearchFilterViewModel>();

            Dictionary<string, IEnumerable<string>> selectedValuesDictionary = selectedValues.ToDictionary();

            foreach (var value in allAttributeValues)
            {
                bool valueIsSelected = false;

                if (selectedValuesDictionary != null)
                {
                    if (selectedValuesDictionary.ContainsKey(attribueName.ToLower()))
                    {
                        IEnumerable<string> parameterValues = selectedValuesDictionary[attribueName.ToLower()];
                        foreach (var parameterValue in parameterValues)
                        {
                            if (parameterValue == value.AttributeValue.ToLower())
                            {
                                valueIsSelected = true;
                                break;
                            }
                        }
                    }
                }
                valuesList.Add(new AttributeValueForSearchFilterViewModel { AttributeValue = value.AttributeValue, IsSelected = valueIsSelected });
            }
            return valuesList;
        }
    }
}