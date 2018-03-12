using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class DeliveryOptionsViewModel
    {
        public int DeliveryId { get; set; }

        [Display(Name = "Forma dostawy")]
        public string Option { get; set; }

        [Display(Name = "Forma płatności")]
        public PaymentOptionsViewModel PaymentOption { get; set; }

        [Display(Name = "Cena dostawy")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        [Display(Name = "Czas realizacji (dni)")]
        public int RealizationTimeInDays { get; set; }
    }

    public enum PaymentOptionsViewModel
    {
        [Display(Name = "Nie dotyczy")]
        NotApplicable,

        [Display(Name = "Płatne przy odbiorze")]
        CashOnDelivery,

        [Display(Name = "Płatne przelewem")]
        PaymentByTransfer
    }

    public class AdminDeliveryOptionsViewModel : DeliveryOptionsViewModel
    {
        [Display(Name = "Aktywne")]
        public bool IsActive { get; set; }
    }

    public class CrudDeliveryViewModel
    {
        [Required(ErrorMessage = "Pole Forma dostawy jest wymagane")]
        [Display(Name = "Forma dostawy")]
        public string Option { get; set; }

        [Required(ErrorMessage = "Pole Forma płatności jest wymagane")]
        [Display(Name = "Forma płatności")]
        public PaymentOptionsViewModel? PaymentOption { get; set; }

        [Required(ErrorMessage = "Pole Cena dostawy jest wymagane")]
        [Display(Name = "Cena dostawy")]
        [DisplayFormat(DataFormatString = "{0:c}", ApplyFormatInEditMode = false)]
        [RegularExpression(@"(\d+?\,\d{1,2}|\d+?\.\d{1,2})|(\d+)|(\.\d{1,2})|(\,\d{1,2})", ErrorMessage = "Nieprawidłowa wartość")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Pole Czas realizacji (dni) jest wymagane")]
        [RegularExpression(@"\d+", ErrorMessage = "Nieprawidłowa wartość")]
        [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowa wartość")]
        [Display(Name = "Czas realizacji (dni)")]
        public int RealizationTimeInDays { get; set; }

        [Display(Name = "Płatność aktywna")]
        public bool IsActive { get; set; }
    }
}