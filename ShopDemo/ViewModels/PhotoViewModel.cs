using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class PhotoViewModel
    {
        [Required]
        public string PhotoPath { get; set; }

        [Required]
        public string PhotoThumbPath { get; set; }
    }
}