namespace DataAccessLayer.Models
{
    public class OrderDetails
    {
        public int OrderDetailsId { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int ProductQuantity { get; set; }
        public decimal ProductUnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}