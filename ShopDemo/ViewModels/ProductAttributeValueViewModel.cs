using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class ProductAttributeValueViewModel
    {
        public int ProductAttributeValueId { get; set; }
        public string AttributeValue { get; set; }
    }

    public class AddProductAttributeValueViewModel
    {
        [Required]
        public string[] AttributeValues { get; set; }

        [Required]
        public int ProductAttributeId { get; set; }
    }

    public class AttributeValueForSearchFilterViewModel
    {
        public bool IsSelected { get; set; }
        public string AttributeValue { get; set; }
    }

    public class ParentAttributeValueViewModel
    {
        public string AttributeValue { get; set; }
        public bool IsSelected { get; set; }
    }
}