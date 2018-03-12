using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class OrdersIndexViewModel
    {
        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Data realizacji zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? OrderRealizationDate { get; set; }
    }

    public class OrderDetailViewModel
    {
        public OrdersIndexViewModel OrderBaseData { get; set; }

        public UserBaseDataViewModel CustomerData { get; set; }

        public UserAddressViewModel Address { get; set; }

        [Display(Name = "Zamówione produkty")]
        public List<OrderDetailsViewModel> OrderDetails { get; set; }
    }

    public class OrderSummaryViewModel
    {
        [Display(Name = "Sposób dostawy")]
        public DeliveryOptionsViewModel DeliveryOption { get; set; }

        [Display(Name = "Produkty")]
        public List<CartItemViewModel> Products { get; set; }

        [DisplayName("Całkowita wartość zakupów z przesyłką")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalPrice { get { return Products != null ? (Products.Sum(p => p.ProductTotalPrice) + DeliveryOption.Price) : 0; } private set { } }
    }

    public class UpdateOrderSummaryViewModel
    {
        public OrdersIndexViewModel OrderBaseData { get; set; }

        [Display(Name = "Produkty z zamówienia")]
        public List<OrderDetailsViewModel> ProductsFromOrder { get; set; }

        [Display(Name = "Sposób dostawy")]
        public DeliveryOptionsViewModel DeliveryOption { get; set; }

        [Display(Name = "Nowe produkty")]
        public List<CartItemViewModel> Products { get; set; }

        [DisplayName("Całkowita wartość zakupów z przesyłką")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalPrice { get { return Products.Sum(p => p.ProductTotalPrice) + ProductsFromOrder.Sum(p => p.Total) + DeliveryOption.Price; } private set { } }
    }

    public class OrderFailureViewModel
    {
        [HiddenInput]
        public int ProductId { get; set; }

        [DisplayName("Produkt")]
        public string ProductName { get; set; }

        [DisplayName("Zamawiana ilość")]
        public int ProductCount { get; set; }

        [DisplayName("Cena jednostkowa")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductPrice { get; set; }

        [DisplayName("Suma")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ProductTotalPrice { get; set; }

        [Display(Name = "Szczegóły błędu")]
        public string Error { get; set; }
    }

    public class OrderErrorViewModel
    {
        public List<OrderFailureViewModel> ProductErrors { get; set; }

        public string OrderStatusError { get; set; }
    }

    public enum OrderStatusViewModel
    {
        [Display(Name = "Niezrealizowane")]
        Uncompleted,

        [Display(Name = "Oczekiwanie na płatność")]
        WaitingForPayment,

        [Display(Name = "Oczekiwanie na finalizacje płatności")]
        WaitingForPaymentComplete,

        [Display(Name = "Zakończone")]
        Completed,

        [Display(Name = "Anulowane przez kupującego")]
        CancelledByCustomer,

        [Display(Name = "W trakcie realizacji")]
        DuringRealization,

        [Display(Name = "Anulowane przez sprzedającego")]
        CancelledByAdmin,

        [Display(Name = "Płatność anulowana")]
        PaymentCancelled,

        [Display(Name = "Płatność odrzucona")]
        PaymentRejected
    }

    public class AdminOrderViewModel
    {
        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }
    }

    public class RealizeOrderViewModel
    {
        public OrdersIndexViewModel OrderBaseData { get; set; }

        public DeliveryOptionsViewModel Delivery { get; set; }

        [Display(Name = "Zamówione produkty")]
        public List<SelectedOrderDetailsViewModel> OrderDetails { get; set; }
    }

    public class UncompleteOrdersViewModel
    {
        public int DeliveryId { get; set; }

        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }
    }

    public class OrderBillViewModel
    {
        public string ShopName { get; set; }
        public string ShopStreet { get; set; }
        public string ShopCity { get; set; }

        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Data realizacji zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderRealizationDate { get; set; }

        [Display(Name = "Do zapłaty")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }

        public PaymentOptions PaymentOption { get; set; }

        public CustomerDataViewModel Customer { get; set; }
        
        public List<OrderDetailsViewModel> OrderDetails { get; set; }
    }

    public class AdminCancelOrder
    {
        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }

        public PaymentOptions PaymentOption { get; set; }

        [Display(Name = "Wiadomość")]
        [Required(ErrorMessage = "Wpisz treść wiadomości")]
        public string Message { get; set; }
    }

    public class UserOrdersListViewModel
    {
        [Display(Name = "Status zamówienia")]
        public OrderStatusViewModel OrderStatus { get; set; }

        public List<UserOrderViewModel> UserOrders { get; set; }
    }

    public class UserOrderViewModel
    {
        [Display(Name = "Nr zamówienia")]
        public int OrderId { get; set; }

        [Display(Name = "Data zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Kwota zamówienia")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Data realizacji zamówienia")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? OrderRealizationDate { get; set; }
    }
}