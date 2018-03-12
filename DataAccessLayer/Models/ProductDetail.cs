using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class ProductDetail
    {
        public int ProductDetailId { get; set; }
        public string DetailValue { get; set; }
        public string DetailName { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
