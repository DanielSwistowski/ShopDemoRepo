using System;

namespace DataAccessLayer.Models
{
    public class ProductRate
    {
        public int ProductRateId { get; set; }
        public int Rate { get; set; }
        public string Comment { get; set; }
        public string NickName { get; set; }
        public DateTime CreatedAt { get; set; }

        //public int UserId { get; set; }
        //public virtual ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}