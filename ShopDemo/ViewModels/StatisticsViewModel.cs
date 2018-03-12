using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class ProductSaleStatisticsViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Produkt")]
        public string ProductName { get; set; }

        [Display(Name = "Ilość sprzedanych")]
        public int SaleQuantity { get; set; }

        [Display(Name = "Średnia cena za szt.")]
        [DataType(DataType.Currency)]
        public decimal AverageProductUnitPrice { get; set; }

        [Display(Name = "Suma")]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
    }

    public class OrderStatisticsViewModel
    {
        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Data realizacji zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? OrderRealizationDate { get; set; }

        [Display(Name = "Ilość produktów")]
        public int OrderedProductCount { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }
    }

    public class ProductOrderHistoryViewModel
    {
        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }

        [Display(Name = "Ilość szt. wybranego produktu")]
        public int OrderedProductCount { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }
    }

    public class BestCustomersViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Całkowita wartość zamówień")]
        [DataType(DataType.Currency)]
        public decimal UserOrdersTotalPrice { get; set; }
    }

    public class MostPopularDeliveryOptionsViewModel
    {
        [Display(Name = "Forma dostawy")]
        public string Option { get; set; }

        [Display(Name = "Forma płatności")]
        public PaymentOptionsViewModel PaymentOption { get; set; }

        [Display(Name = "Cena dostawy")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        [Display(Name = "Liczba zamówień")]
        public int OrdersCount { get; set; }
    }

    public class SalesSummaryViewModel
    {
        [Display(Name = "Miesiąc")]
        public string MonthName { get; set; }

        [Display(Name = "Rok")]
        public int Year { get; set; }

        [Display(Name = "Kwota")]
        [DataType(DataType.Currency)]
        public decimal Summary { get; set; }
    }

    public class TopRatedProductsViewModel
    {
        [ScaffoldColumn(false)]
        public int ProductId { get; set; }

        [Display(Name ="Produkt")]
        public string ProductName { get; set; }

        [Display(Name = "Ilość ocen")]
        public int RatesCount { get; set; }

        [Display(Name = "Średnia ocena")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public double AverageRate { get; set; }
    }
}