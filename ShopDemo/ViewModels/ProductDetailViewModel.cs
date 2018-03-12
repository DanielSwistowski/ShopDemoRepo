using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class ProductDetailViewModel
    {
        [Required]
        public string DetailName { get; set; }

        [Required]
        public string DetailValue { get; set; }
    }
}