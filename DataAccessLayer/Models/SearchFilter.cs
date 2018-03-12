using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class SearchFilter
    {
        [Key]
        [ForeignKey("ProductAttribute")]
        public int ProductAttributeId { get; set; }
        public virtual ProductAttribute ProductAttribute { get; set; }

        public FilterType? FilterType { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }

    public enum FilterType
    {
        //[Display(Name = "Lista")]
        //List,

        [Display(Name = "Lista wielokrotnego wyboru")]
        CheckboxList

        //[Display(Name = "Zakres od do")]
        //Range
    }
}