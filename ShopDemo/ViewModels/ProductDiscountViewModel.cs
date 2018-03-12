using System;
using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class ProductDiscountViewModel
    {
        public BasicProductDataViewModel BasicProductDataViewModel { get; set; }

        [Display(Name = "Wysokość rabatu (%)")]
        [Range(1, 99, ErrorMessage = "Rabat może wynosić od 1% do 99%")]
        public int DiscountQuantity { get; set; }
    }

    public class AddProductDiscountViewModel : ProductDiscountViewModel
    {
        [Display(Name = "Początek promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime DiscountStartTime { get; set; }

        [Display(Name = "Zakończenie promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime DiscountEndTime { get; set; }
    }

    public class EditProductDiscountViewModel : ProductDiscountViewModel
    {
        [Display(Name = "Początek promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DiscountStartTime { get; set; }

        [Display(Name = "Zakończenie promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DiscountEndTime { get; set; }

        [Display(Name = "Cena promocyjna")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        public decimal PromotionPrice { get; set; }
    }

    public class ProductDiscountDetailsViewModel : ProductDiscountViewModel
    {
        [Display(Name = "Status promocji")]
        public ProductDiscountStatusViewModel Status { get; set; }

        [Display(Name = "Początek promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime DiscountStartTime { get; set; }

        [Display(Name = "Zakończenie promocji")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = false)]
        public DateTime DiscountEndTime { get; set; }

        [Display(Name = "Cena promocyjna")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        public decimal PromotionPrice { get; set; }
    }

    public enum ProductDiscountStatusViewModel
    {
        [Display(Name ="Nierozpoczęta")]
        WaitingForStart,

        [Display(Name = "Aktualnie trwa")]
        DuringTime,

        [Display(Name = "Zakończona")]
        Ended
    }
}