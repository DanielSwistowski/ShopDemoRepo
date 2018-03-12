using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ProductCount { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductTotalPrice { get { return ProductCount > 0 ? ProductCount * ProductPrice : 0; } private set { } }
    }

    public class ShoppingCart
    {
        //public string ShoppingCartId { get; set; }
        public List<CartItem> CartItems { get; set; }
        public int TotalItemsCount { get { return CartItems != null ? CartItems.Count : 0; } }
        public decimal TotalPrice { get { return CartItems != null ? CartItems.Sum(p => p.ProductTotalPrice): 0; } }
    }
}