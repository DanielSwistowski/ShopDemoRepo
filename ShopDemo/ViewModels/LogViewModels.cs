using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShopDemo.ViewModels
{
    public class IndexLogViewModel
    {
        [DisplayName("Plik")]
        public IEnumerable<SelectListItem> Files { get; set; }
    }

    public class LogFileDetailsViewModel
    {
        [DisplayName("Data")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime Time { get; set; }

        [DisplayName("Typ")]
        public string Level { get; set; }

        [DisplayName("Komunikat")]
        public string Message { get; set; }
    }
}