using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models.Payu
{
    public class PayuAuth
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public string Expires_in { get; set; }
        public string Grant_type { get; set; }
    }

    public class OrderCreateRequest
    {
        public string NotifyUrl { get; set; }//adres do powiadomień o statusach

        public string ExtOrderId { get; set; }//Identyfikator zamówienia w systemie sprzedawcy

        public string CustomerIp { get; set; }//IP kupujacego

        public string Description { get; set; }//opis zamowienia

        public long TotalAmount { get; set; }

        public string ContinueUrl { get; set; }//strona po zakonczeniu platnosci

        public Buyer Buyer { get; set; }

        public List<Product> Products { get; set; }
    }

    public class Buyer
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCreateResponse
    {
        public Status Status { get; set; }
        public string RedirectUri { get; set; }
        public string OrderId { get; set; }
        public string ExtOrderId { get; set; }
    }

    public class Order //notify about order status
    {
        public string OrderId { get; set; }
        public string ExtOrderId { get; set; }
        public DateTime OrderCreateDate { get; set; }
        public string NotifyUrl { get; set; }
        public string MustomerIp { get; set; }
        public string MerchantPosId { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }
        public string TotalAmount { get; set; }
        public Buyer Buyer { get; set; }
        public Paymethod PayMethod { get; set; }
        public Product[] Products { get; set; }
        public string Status { get; set; }
    }

    public class Paymethod
    {
        public string Type { get; set; }
    }


    public class RefundOrderResponse
    {
        public string orderId { get; set; }
        public Refund refund { get; set; }
        public Status status { get; set; }
    }

    public class Refund
    {
        public string refundId { get; set; }
        public string extRefundId { get; set; }
        public string amount { get; set; }
        public string currencyCode { get; set; }
        public string description { get; set; }
        public DateTime creationDateTime { get; set; }
        public string status { get; set; }
        public DateTime statusDateTime { get; set; }
    }

    public class Status
    {
        public string statusCode { get; set; }
        public string statusDesc { get; set; }
        public string code { get; set; }
        public string codeLiteral { get; set; }
    }

    public class AcceptPayment
    {
        public Status status { get; set; }
    }
}