using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class ViewModelBase
    {
        [HiddenInput(DisplayValue = false)]
        public string ReturnUrl { get; set; }
    }
}