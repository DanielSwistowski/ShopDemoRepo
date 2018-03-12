using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class ProductDiscount
    {
        public int DiscountQuantity { get; set; }
        public DateTime DiscountStartTime { get; set; }
        public DateTime DiscountEndTime { get; set; }

        public ProductDiscountStatus Status { get; set; }
        public string StartDiscountJobId { get; set; }
        public string StopDiscountJobId { get; set; }

        [Key]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }

    public enum ProductDiscountStatus
    {
        WaitingForStart,
        DuringTime,
        Ended
    }
}