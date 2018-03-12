using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class Product
    {
        public Product()
        {
            ProductGallery = new HashSet<Photo>();
            OrderDetails = new HashSet<OrderDetails>();
            ProductRates = new HashSet<ProductRate>();
            ProductDetails = new HashSet<ProductDetail>();
            ProductCategory = new HashSet<ProductCategory>();
        }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        [ConcurrencyCheck]
        public int Quantity { get; set; }

        [ConcurrencyCheck]
        public bool IsInOffer { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedFromOfferDate { get; set; }


        public virtual ICollection<Photo> ProductGallery { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
        public virtual ICollection<ProductRate> ProductRates { get; set; }
        public virtual ICollection<ProductCategory> ProductCategory { get; set; }
        public virtual ICollection<ProductDetail> ProductDetails { get; set; }
        public virtual ProductDiscount ProductDiscount { get; set; }
    }
}
