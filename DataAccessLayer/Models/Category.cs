using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Category
    {
        public Category()
        {
            ProductCategory = new HashSet<ProductCategory>();
            ProductAttributes = new HashSet<ProductAttribute>();
        }

        public int CategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ProductCategory> ProductCategory { get; set; }
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
    }
}
