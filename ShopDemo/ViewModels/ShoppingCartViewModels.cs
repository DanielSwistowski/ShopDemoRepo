using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }

        [DisplayName("Całkowita ilość")]
        public int TotalItemsCount { get; set; }

        [DisplayName("Całkowita wartość zakupów")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalPrice { get; set; }
    }

    public class CartItemViewModel
    {
        [HiddenInput]
        public int ProductId { get; set; }

        [DisplayName("Produkt")]
        public string ProductName { get; set; }

        [DisplayName("Ilość")]
        [Required(ErrorMessage ="Pole wymagane")]
        [Range(0,100,ErrorMessage ="Nieprawidłowa ilość")]
        public int ProductCount { get; set; }

        [DisplayName("Cena jednostkowa")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductPrice { get; set; }

        [DisplayName("Suma")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductTotalPrice { get; set; }
    }
}