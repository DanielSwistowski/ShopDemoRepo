using ShopDemo.CustomValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int ProductId { get; set; }

        [Display(Name ="Nazwa produktu")]
        public string ProductName { get; set; }

        [Display(Name ="Ilość")]
        public int ProductQuantity { get; set; }

        [Display(Name = "Cena jednostkowa")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductUnitPrice { get; set; }

        [Display(Name = "Suma")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Total { get; set; }
    }

    public class SelectedOrderDetailsViewModel
    {
        [MustBeTrue(ErrorMessage ="Wybierz wszystkie produkty z listy")]
        public bool Selected { get; set; }

        public int OrderDetailsId { get; set; }

        [Display(Name = "Nazwa produktu")]
        public string ProductName { get; set; }

        [Display(Name = "Ilość")]
        public int ProductQuantity { get; set; }

        [Display(Name = "Cena jednostkowa")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductUnitPrice { get; set; }

        [Display(Name = "Suma")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Total { get; set; }
    }
}