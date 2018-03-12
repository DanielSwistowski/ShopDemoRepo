using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Name { get; set; }
    }

    public class AddCategoryViewModel
    {
        public int? ParentCategoryId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class EditCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class CategoryDropDownViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class SelectedCategoryViewModel
    {
        public int SelectedCategoryId { get; set; }
        public List<CategoryDropDownViewModel> Categories { get; set; }
    }

    public class CategoryMenuBaseViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string SearchFilter { get; set; }
    }

    public class CategoryMenuViewModel
    {
        public string AreaName { get; set; }
        public string ActionName { get; set; }
        public IEnumerable<CategoryMenuBaseViewModel> Categories { get; set; }
    }

    public class BaseCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}