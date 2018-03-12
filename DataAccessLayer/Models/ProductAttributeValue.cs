using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class ProductAttributeValue
    {
        public int ProductAttributeValueId { get; set; }
        public string AttributeValue { get; set; }

        public int ProductAttributeId { get; set; }
        public virtual ProductAttribute ProductAttribute { get; set; }
    }
}
