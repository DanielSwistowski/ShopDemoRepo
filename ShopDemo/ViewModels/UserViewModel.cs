using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopDemo.ViewModels
{
    public class UsersListViewModel
    {
        public int Id { get; set; }

        [Display(Name ="Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Rejestracja potwierdzona")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Status konta")]
        public bool AccountStatus { get; set; }
    }

    public class SendEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Odbiorca")]
        public string EmailTo { get; set; }

        [Display(Name ="Temat")]
        [Required(ErrorMessage ="Wpisz temat wiadomości")]
        public string Subject { get; set; }

        [Display(Name ="Wiadomość")]
        [Required(ErrorMessage = "Wpisz treść wiadomości")]
        public string Message { get; set; }
    }

    public class UserDetailsVewModel
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Rejestracja potwierdzona")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Ilość błędnych prób logowania")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "Termin blokady konta")]
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool AccountIsEnabled { get; set; }

        [Display(Name = "Uprawnienia")]
        public IEnumerable<string> Roles { get; set; }

        public UserAddressViewModel Address { get; set; }
    }

    public class UserAddressViewModel
    {
        [Display(Name ="Miejscowość")]
        public string City { get; set; }

        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [Display(Name = "Nr domu/mieszkania")]
        public string HouseNumber { get; set; }

        [Display(Name = "Kod pocztowy")]
        public string ZipCode { get; set; }
    }

    public class LockUserAccountViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        
        [Display(Name = "Powód blokady konta")]
        [Required(ErrorMessage = "Wpisz powód blokady konta")]
        public string LockReason { get; set; }

        [Display(Name = "Temat")]
        [Required]
        public string Subject { get; set; }
    }

    public class UnlockUserAccountViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Wiadomość do użytkownika")]
        [Required(ErrorMessage = "Wpisz treść wiadomości")]
        public string Message { get; set; }

        [Display(Name = "Temat")]
        [Required]
        public string Subject { get; set; }
    }

    public class UserBaseDataViewModel
    {
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Status konta")]
        public bool AccountStatus { get; set; }
    }

    public class CustomerDataViewModel
    {
        public string FullName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
    }
}