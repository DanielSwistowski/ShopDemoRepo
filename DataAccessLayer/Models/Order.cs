using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetails>();
        }

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? OrderRealizationDate { get; set; }

        [ConcurrencyCheck]
        public OrderStatus OrderStatus { get; set; }

        public decimal TotalAmount { get; set; }

        public bool Removed { get; set; } // not display order on user orders list

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int DeliveryId { get; set; }
        public virtual Delivery DeliveryOption { get; set; }

        public virtual PayuOrderData PayuData { get; set; }

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }

    public enum OrderStatus
    {
        Uncompleted,
        WaitingForPayment,
        WaitingForPaymentComplete,
        Completed,
        CancelledByCustomer,
        DuringRealization,
        CancelledByAdmin,
        PaymentCancelled,
        PaymentRejected
    }
}