using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class SendEmailToAdminViewModel
    {
        [Required(ErrorMessage = "Wpisz swój adres e-mail")]
        [EmailAddress]
        [Display(Name = "Twój adres e-mail")]
        public string EmailFrom { get; set; }

        [Display(Name = "Temat wiadomości")]
        [Required(ErrorMessage = "Wpisz temat wiadomości")]
        public string Subject { get; set; }

        [Display(Name = "Treść")]
        [Required(ErrorMessage = "Wpisz treść wiadomości")]
        public string Message { get; set; }
    }
}