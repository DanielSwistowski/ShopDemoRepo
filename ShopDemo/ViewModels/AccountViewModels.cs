using System.ComponentModel.DataAnnotations;

namespace ShopDemo.ViewModels
{
    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [Display(Name = "E-mail")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [Display(Name = "Zapamiętaj mnie?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Pole Imię jest wymagane")]
        [Display(Name ="Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Pole Nazwisko jest wymagane")]
        [Display(Name ="Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi zawierać przynajmniej 6 znaków, nie więcej niż 100")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Podane hasła się nie zgadzają")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi zawierać przynajmniej 6 znaków, nie więcej niż 100")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Podane hasła się nie zgadzają")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class RestoreActivationLinkViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class AccountIsLockedViewModel
    {
        [Required(ErrorMessage = "Pole E-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "Wpisz swój e-mail")]
        public string Email { get; set; }
    }
}
