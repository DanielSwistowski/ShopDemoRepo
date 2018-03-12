using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public UserAddressViewModel Address { get; set; }
        public UserPersonalDataViewModel PersonalData { get; set; }
    }
    
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Pole Aktualne hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Pole Nowe hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło")]
        [Compare("NewPassword", ErrorMessage = "Podane hasła się nie zgadzają")]
        public string ConfirmPassword { get; set; }
    }

    public class UserPersonalDataViewModel
    {
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Uprawnienia")]
        public IEnumerable<string> Roles { get; set; }
    }

    public class EditPersonalDataViewModel
    {
        [Required(ErrorMessage = "Pole Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Pole Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class EditAddressViewModel
    {
        [Required(ErrorMessage = "Pole Miejscowość jest wymagane")]
        [Display(Name = "Miejscowość")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pole Ulica jest wymagane")]
        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Pole Nr domu/mieszkania jest wymagane")]
        [Display(Name = "Nr domu/mieszkania")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Pole Kod pocztowy jest wymagane")]
        [RegularExpression(@"[0-9]{2}(?:-[0-9]{3})",ErrorMessage = "Błędny kod pocztowy. Poprawny format xx-xxx")]
        [Display(Name = "Kod pocztowy")]
        public string ZipCode { get; set; }
    }
}