using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class PayuOrderData
    {
        [Key]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public string PaymentStatus { get; set; }
        public string PayuOrderId { get; set; }
    }
}
