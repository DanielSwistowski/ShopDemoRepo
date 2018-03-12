using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class ProductAttributeViewModel
    {
        public int ProductAttributeId { get; set; }
        public string Name { get; set; }
    }

    public class ParentCategoryProductAtributeViewModel
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class EditProductAttributeViewModel
    {
        [Required]
        public int ProductAttributeId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class AddProductAttributesViewModel
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string[] Attributes { get; set; }
    }

    public class ProductAttributeWithValuesViewModel
    {
        public string Name { get; set; }
        public IEnumerable<string> AttributeValues { get; set; }
    }
}