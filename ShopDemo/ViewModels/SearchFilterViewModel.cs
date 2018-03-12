using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class SearchFilterViewModel
    {
        public int ProductAttributeId { get; set; }
        public string Attribute { get; set; }
        public FilterType? FilterType { get; set; }
    }

    public class AddSearchFilterViewModel
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int ProductAttributeId { get; set; }
        [Required]
        public FilterType? FilterType { get; set; }

        public IEnumerable<SelectListItem> AttributesList { get; set; }
    }

    public class SearchFilterListAndAddFilterViewModel
    {
        public IEnumerable<SearchFilterViewModel> SearchFiltersList { get; set; }
        public AddSearchFilterViewModel AddSearchFilterModel { get; set; }
    }

    public class SearchFiltersForProductViewModel
    {
        public string Attribute { get; set; }
        public FilterType? FilterType { get; set; }
        public IEnumerable<AttributeValueForSearchFilterViewModel> AttributeValues { get; set; }
    }
}