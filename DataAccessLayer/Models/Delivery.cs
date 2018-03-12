using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class Delivery
    {
        public Delivery()
        {
            Orders = new HashSet<Order>();
        }

        public int DeliveryId { get; set; }
        public string Option { get; set; }
        public PaymentOptions PaymentOption { get; set; }
        public decimal Price { get; set; }
        public int RealizationTimeInDays { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    public enum PaymentOptions
    {
        NotApplicable,
        
        CashOnDelivery,
        
        PaymentByTransfer
    }
}