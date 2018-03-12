using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class ProductRateViewModel
    {
        public int ProductRateId { get; set; }
        public int Rate { get; set; }
        public string Comment { get; set; }
        public string NickName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }
    }

    public class ProductRateListViewModel
    {
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public double TotalRate { get; set; }
        public List<ProductRateViewModel> Comments { get; set; }
    }

    public class AddCommentViewModel
    {
        public int ProductId { get; set; }
        public string Comment { get; set; }
        public int Rate { get; set; }
        public string NickName { get; set; }
    }
}